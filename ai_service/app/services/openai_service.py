import json
import re
import logging
import asyncio
from openai import AsyncOpenAI, APIStatusError
from app.core.config import settings

logger = logging.getLogger(__name__)

# Initialize the async OpenAI client once at module load
_client = AsyncOpenAI(api_key=settings.OPENAI_API_KEY)

_MODEL = "gpt-4o-mini"

# Limit concurrent calls to prevent RPM burst 429 errors
_request_semaphore = asyncio.Semaphore(2)

# Retry config for 429 rate-limit errors
_MAX_RETRIES = 3
_BASE_RETRY_DELAY = 5  # seconds, exponential backoff


def _extract_json(text: str) -> dict:
    """Strip markdown fences and parse JSON from model response."""
    text = text.strip()
    text = re.sub(r"^```(?:json)?\s*", "", text)
    text = re.sub(r"\s*```$", "", text)
    return json.loads(text)


def _is_rate_limit_error(e: Exception) -> bool:
    """Check if an exception is a transient 429 rate-limit error."""
    if isinstance(e, APIStatusError):
        return e.status_code == 429
    return "429" in str(e)


def _is_fatal_error(e: Exception) -> bool:
    """Check for fatal errors like invalid API key or exhausted quota that should not be retried."""
    if isinstance(e, APIStatusError):
        return e.status_code in (401, 403)
    error_str = str(e).lower()
    return "invalid api key" in error_str or "insufficient_quota" in error_str or "quota" in error_str


async def generate_json(prompt: str) -> dict:
    """Call OpenAI and parse a structured JSON response. Retries up to 3 times on transient rate-limit."""
    last_error = None
    async with _request_semaphore:
        for attempt in range(1, _MAX_RETRIES + 1):
            try:
                response = await _client.chat.completions.create(
                    model=_MODEL,
                    messages=[
                        {
                            "role": "system",
                            "content": "You are a helpful travel planning assistant. Always respond with valid JSON only, no additional text or markdown.",
                        },
                        {"role": "user", "content": prompt},
                    ],
                    response_format={"type": "json_object"},
                )
                raw = response.choices[0].message.content
                return json.loads(raw)
            except json.JSONDecodeError as e:
                logger.error(f"JSON parse error from OpenAI: {e}")
                raise ValueError(f"AI returned invalid JSON: {e}")
            except Exception as e:
                if _is_fatal_error(e):
                    logger.error(f"Fatal OpenAI API error (no retry): {e}")
                    raise e
                if _is_rate_limit_error(e) and attempt < _MAX_RETRIES:
                    delay = _BASE_RETRY_DELAY * attempt
                    logger.warning(
                        f"Rate limit hit (attempt {attempt}/{_MAX_RETRIES}). "
                        f"Retrying in {delay}s..."
                    )
                    await asyncio.sleep(delay)
                    last_error = e
                    continue
                logger.error(f"OpenAI API error: {e}")
                raise

    raise last_error


async def generate_chat_reply(system_prompt: str, history: list[dict]) -> str:
    """
    Multi-turn chat with OpenAI.
    history: list of {"role": "user"|"assistant", "content": str}
    """
    last_error = None
    async with _request_semaphore:
        for attempt in range(1, _MAX_RETRIES + 1):
            try:
                messages = [{"role": "system", "content": system_prompt}] + [
                    {"role": msg["role"], "content": msg["content"]}
                    for msg in history
                ]
                response = await _client.chat.completions.create(
                    model=_MODEL,
                    messages=messages,
                )
                return response.choices[0].message.content.strip()
            except Exception as e:
                if _is_fatal_error(e):
                    logger.error(f"Fatal OpenAI chat error (no retry): {e}")
                    raise e
                if _is_rate_limit_error(e) and attempt < _MAX_RETRIES:
                    delay = _BASE_RETRY_DELAY * attempt
                    logger.warning(
                        f"Chat rate limit hit (attempt {attempt}/{_MAX_RETRIES}). "
                        f"Retrying in {delay}s..."
                    )
                    await asyncio.sleep(delay)
                    last_error = e
                    continue
                logger.error(f"OpenAI chat error: {e}")
                raise

    raise last_error
