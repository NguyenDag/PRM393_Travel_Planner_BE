import json
import re
import time
import logging
from google import genai
from google.genai import types
from app.core.config import settings

logger = logging.getLogger(__name__)

# Initialize the new SDK client once at module load
_client = genai.Client(api_key=settings.GEMINI_API_KEY)

_MODEL = "gemini-2.5-flash"

# Retry config for 429 rate-limit errors
_MAX_RETRIES = 3
_RETRY_DELAY_SECONDS = 30  # wait 30s between retries


def _extract_json(text: str) -> dict:
    """Strip markdown fences and parse JSON from model response."""
    text = text.strip()
    text = re.sub(r"^```(?:json)?\s*", "", text)
    text = re.sub(r"\s*```$", "", text)
    return json.loads(text)


def _is_rate_limit_error(e: Exception) -> bool:
    """Check if an exception is a 429 rate-limit error."""
    return "429" in str(e) or "RESOURCE_EXHAUSTED" in str(e)


async def generate_json(prompt: str) -> dict:
    """Call Gemini and parse a structured JSON response. Retries up to 3 times on rate-limit."""
    last_error = None
    for attempt in range(1, _MAX_RETRIES + 1):
        try:
            response = _client.models.generate_content(
                model=_MODEL,
                contents=prompt,
            )
            return _extract_json(response.text)
        except json.JSONDecodeError as e:
            logger.error(f"JSON parse error from Gemini: {e}\nRaw: {response.text[:500]}")
            raise ValueError(f"AI returned invalid JSON: {e}")
        except Exception as e:
            if _is_rate_limit_error(e) and attempt < _MAX_RETRIES:
                logger.warning(
                    f"Rate limit hit (attempt {attempt}/{_MAX_RETRIES}). "
                    f"Retrying in {_RETRY_DELAY_SECONDS}s..."
                )
                time.sleep(_RETRY_DELAY_SECONDS)
                last_error = e
                continue
            logger.error(f"Gemini API error: {e}")
            raise

    raise last_error


async def generate_chat_reply(system_prompt: str, history: list[dict]) -> str:
    """
    Multi-turn chat with Gemini using the new SDK.
    history: list of {"role": "user"|"model", "parts": [str]}
    """
    last_error = None
    for attempt in range(1, _MAX_RETRIES + 1):
        try:
            full_history = [
                types.Content(
                    role=msg["role"],
                    parts=[types.Part(text=msg["parts"][0])],
                )
                for msg in history
            ]

            response = _client.models.generate_content(
                model=_MODEL,
                contents=full_history,
                config=types.GenerateContentConfig(
                    system_instruction=system_prompt,
                ),
            )
            return response.text.strip()
        except Exception as e:
            if _is_rate_limit_error(e) and attempt < _MAX_RETRIES:
                logger.warning(
                    f"Chat rate limit hit (attempt {attempt}/{_MAX_RETRIES}). "
                    f"Retrying in {_RETRY_DELAY_SECONDS}s..."
                )
                time.sleep(_RETRY_DELAY_SECONDS)
                last_error = e
                continue
            logger.error(f"Gemini chat error: {e}")
            raise

    raise last_error
