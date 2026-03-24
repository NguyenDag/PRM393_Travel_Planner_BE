import logging
from app.prompts.recommendation_prompt import build_recommendation_prompt
from app.services import openai_service
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
    try:
        data = await openai_service.generate_json(prompt)
        set_cache(cache_key, data, ttl_seconds=CACHE_TTL)
        return data
    except Exception as e:
        logger.error(f"Failed to generate AI recommendations: {e}. Returning fallback data.")
        # Fallback high-quality mock data to prevent UI crash
        fallback = {
            "destinations": [
                {
                    "destination": "Đà Lạt",
                    "highlight": "Thành phố ngàn hoa với khí hậu mát mẻ quanh năm.",
                    "best_for": "Nghỉ dưỡng, Cặp đôi",
                    "estimated_budget": "Trung bình",
                    "best_season": "Quanh năm",
                    "why_recommended": "Phù hợp với sở thích văn hóa và khí hậu mùa " + season
                },
                {
                    "destination": "Hội An",
                    "highlight": "Phố cổ lung linh ánh đèn lồng và kiến trúc di sản.",
                    "best_for": "Văn hóa, Ẩm thực",
                    "estimated_budget": "Thấp - Trung bình",
                    "best_season": "Mùa xuân, Mùa hè",
                    "why_recommended": "Điểm đến văn hóa hàng đầu Việt Nam."
                },
                {
                    "destination": "Sapa",
                    "highlight": "Ruộng bậc thang hùng vĩ và văn hóa dân tộc độc đáo.",
                    "best_for": "Khám phá, Thiên nhiên",
                    "estimated_budget": "Trung bình",
                    "best_season": "Mùa thu, Mùa xuân",
                    "why_recommended": "Cảnh quan thiên nhiên tuyệt đẹp và bản sắc văn hóa."
                }
            ]
        }
        return fallback
