import logging
from app.schemas.itinerary_schema import ItineraryRequest, ItineraryResponse
from app.prompts.itinerary_prompt import build_itinerary_prompt
from app.services import gemini_service
from app.core.cache import get_cache, set_cache, make_cache_key

logger = logging.getLogger(__name__)

CACHE_TTL = 60 * 60 * 6  # 6 hours — itineraries don't change often


async def generate(request: ItineraryRequest) -> ItineraryResponse:
    cache_key = make_cache_key(
        "itinerary",
        {
            "destination": request.destination,
            "days": request.days,
            "budget": request.budget,
            "style": request.style,
            "notes": request.notes,
        },
    )

    # Try cache first
    cached = get_cache(cache_key)
    if cached:
        logger.info(f"Cache hit for itinerary: {request.destination}")
        return ItineraryResponse(**cached)

    # Build prompt and call Gemini
    prompt = build_itinerary_prompt(
        destination=request.destination,
        days=request.days,
        budget=request.budget.value,
        style=request.style.value,
        notes=request.notes or "",
    )

    logger.info(f"Generating itinerary for {request.destination} ({request.days} days, {request.style})")
    data = await gemini_service.generate_json(prompt)

    result = ItineraryResponse(**data)

    # Save to cache
    set_cache(cache_key, data, ttl_seconds=CACHE_TTL)

    return result
