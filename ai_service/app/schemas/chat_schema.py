from pydantic import BaseModel, Field
from typing import List, Optional


class Message(BaseModel):
    role: str = Field(..., pattern="^(user|assistant)$")
    content: str


class TripContext(BaseModel):
    destination: Optional[str] = None
    current_day: Optional[int] = None
    current_activity: Optional[str] = None
    current_time: Optional[str] = None


class ChatRequest(BaseModel):
    messages: List[Message] = Field(..., min_length=1)
    trip_context: Optional[TripContext] = None


class ChatResponse(BaseModel):
    reply: str
