from fastapi import APIRouter, HTTPException, Depends
from app.schemas.chat_schema import ChatRequest, ChatResponse
from app.services import chat_service
from app.core.security import verify_internal_token

router = APIRouter()


@router.post(
    "/chat",
    response_model=ChatResponse,
    summary="Intelligent travel assistant chatbot",
    description="Multi-turn conversational AI that helps users with travel questions, alternatives, and trip adjustments. Supports optional trip context for smarter, context-aware replies.",
)
async def chat(
    request: ChatRequest,
    _: str = Depends(verify_internal_token),
):
    try:
        return await chat_service.chat(request)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Chat service error: {str(e)}")
