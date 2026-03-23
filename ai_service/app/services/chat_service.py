import logging
from app.schemas.chat_schema import ChatRequest, ChatResponse
from app.prompts.recommendation_prompt import build_chat_system_prompt
from app.services import gemini_service

logger = logging.getLogger(__name__)


def _to_gemini_history(messages: list, system_prompt: str) -> list[dict]:
    """
    Convert our message list to Gemini's chat history format.
    Gemini roles: "user" | "model"
    We inject the system prompt as the first user turn (Gemini 1.5 supports system instructions
    natively but this approach is backward-compatible).
    """
    history = [
        {"role": "user", "parts": [system_prompt]},
        {"role": "model", "parts": ["Xin chào! Tôi là trợ lý du lịch Du Xuan. Tôi có thể giúp gì cho bạn?"]},
    ]

    for msg in messages:
        role = "model" if msg.role == "assistant" else "user"
        history.append({"role": role, "parts": [msg.content]})

    return history


async def chat(request: ChatRequest) -> ChatResponse:
    context_dict = request.trip_context.model_dump() if request.trip_context else None
    system_prompt = build_chat_system_prompt(context_dict)

    history = _to_gemini_history(request.messages, system_prompt)

    logger.info(f"Chat request with {len(request.messages)} message(s)")
    reply = await gemini_service.generate_chat_reply(system_prompt, history)

    return ChatResponse(reply=reply)
