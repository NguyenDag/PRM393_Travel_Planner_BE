from pydantic import BaseModel, Field
from typing import List, Optional
from enum import Enum


class TravelStyle(str, Enum):
    adventure = "Adventure"
    relaxing = "Relaxing"
    cultural = "Cultural"


class BudgetLevel(str, Enum):
    low = "low"
    medium = "medium"
    high = "high"


class ItineraryRequest(BaseModel):
    destination: str = Field(..., examples=["Đà Lạt"])
    days: int = Field(..., ge=1, le=30, examples=[3])
    budget: BudgetLevel = Field(..., examples=["medium"])
    style: TravelStyle = Field(..., examples=["Cultural"])
    notes: Optional[str] = Field(None, examples=["Tôi thích các điểm ít người biết"])


class Activity(BaseModel):
    time: str = Field(..., example="08:00")
    title: str
    location: str
    description: str
    estimated_cost: str


class DayPlan(BaseModel):
    day_number: int
    activities: List[Activity]


class PackingCategory(BaseModel):
    category: str
    items: List[str]


class ItineraryResponse(BaseModel):
    trip_name: str
    destination: str
    image_url: Optional[str] = None
    packing_list: List[PackingCategory] = Field(default_factory=list)
    days: List[DayPlan]
