import logging
from app.schemas.chat_schema import ChatRequest, ChatResponse
from app.prompts.recommendation_prompt import build_chat_system_prompt
from app.services import openai_service

logger = logging.getLogger(__name__)


def _to_openai_history(messages: list, system_prompt: str) -> list[dict]:
    """
    Convert our message list to OpenAI's chat history format.
    OpenAI roles: "user" | "assistant"
    """
    history = [
        {"role": "assistant", "content": "Xin chào! Tôi là trợ lý du lịch Du Xuan. Tôi có thể giúp gì cho bạn?"},
    ]

    for msg in messages:
        role = "assistant" if msg.role == "assistant" else "user"
        history.append({"role": role, "content": msg.content})

    return history


async def chat(request: ChatRequest) -> ChatResponse:
    context_dict = request.trip_context.model_dump() if request.trip_context else None
    system_prompt = build_chat_system_prompt(context_dict)

    history = _to_openai_history(request.messages, system_prompt)

    logger.info(f"Chat request with {len(request.messages)} message(s)")
    reply = await openai_service.generate_chat_reply(system_prompt, history)

    return ChatResponse(reply=reply)
