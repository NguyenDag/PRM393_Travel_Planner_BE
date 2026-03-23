from fastapi import APIRouter, HTTPException, Depends, Query
from typing import List, Optional
from app.services import recommendation_service
from app.core.security import verify_internal_token

router = APIRouter()


@router.get(
    "/recommendations",
    summary="Get AI-powered destination recommendations",
    description="Returns personalized travel destination suggestions based on user profile, interests, and travel history.",
)
async def get_recommendations(
    interests: List[str] = Query(default=[], example=["adventure", "food"]),
    past_destinations: List[str] = Query(default=[], example=["Đà Lạt", "Hội An"]),
    season: str = Query(default="summer", example="summer"),
    budget: str = Query(default="medium", example="medium"),
    _: str = Depends(verify_internal_token),
):
    try:
        return await recommendation_service.get_recommendations(
            interests=interests,
            past_destinations=past_destinations,
            season=season,
            budget=budget,
        )
    except ValueError as e:
        raise HTTPException(status_code=422, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"AI service error: {str(e)}")
