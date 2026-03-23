from fastapi import APIRouter, HTTPException, Depends
from app.schemas.itinerary_schema import ItineraryRequest, ItineraryResponse
from app.services import itinerary_service
from app.core.security import verify_internal_token

router = APIRouter()


@router.post(
    "/generate-itinerary",
    response_model=ItineraryResponse,
    summary="Generate a multi-day travel itinerary",
    description="Uses Gemini AI to create a detailed day-by-day itinerary based on destination, duration, budget, and travel style.",
)
async def generate_itinerary(
    request: ItineraryRequest,
    _: str = Depends(verify_internal_token),
):
    try:
        return await itinerary_service.generate(request)
    except ValueError as e:
        raise HTTPException(status_code=422, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"AI service error: {str(e)}")
