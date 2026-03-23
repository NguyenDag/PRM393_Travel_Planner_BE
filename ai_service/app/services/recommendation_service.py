import logging
from app.prompts.recommendation_prompt import build_recommendation_prompt
from app.services import gemini_service
from app.core.cache import get_cache, set_cache, make_cache_key

logger = logging.getLogger(__name__)

CACHE_TTL = 60 * 60 * 12  # 12 hours


async def get_recommendations(
    interests: list[str],
    past_destinations: list[str],
    season: str,
    budget: str,
) -> dict:
    cache_key = make_cache_key(
        "recommendations",
        {
            "interests": sorted(interests),
            "past": sorted(past_destinations),
            "season": season,
            "budget": budget,
        },
    )

    cached = get_cache(cache_key)
    if cached:
        logger.info("Cache hit for recommendations")
        return cached

    prompt = build_recommendation_prompt(
        interests=interests,
        past_destinations=past_destinations,
        season=season,
        budget=budget,
    )

    logger.info(f"Generating recommendations (season={season}, budget={budget})")
    data = await gemini_service.generate_json(prompt)

    set_cache(cache_key, data, ttl_seconds=CACHE_TTL)
    return data
