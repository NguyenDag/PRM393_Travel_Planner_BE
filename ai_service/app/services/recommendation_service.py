import logging
from app.prompts.recommendation_prompt import build_recommendation_prompt
from app.services import openai_service
from app.services import pexels_service
from app.core.cache import get_cache, set_cache, make_cache_key
import asyncio

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
        
        # Retrofit images to old cache if missing
        if "recommendations" in cached:
            missing_images = [rec for rec in cached["recommendations"] if "image_url" not in rec]
            if missing_images:
                logger.info(f"Retrofitting images for {len(missing_images)} cached recommendations")
                async def fetch_and_assign(rec):
                    image_url = await pexels_service.search_image(rec.get("destination", ""))
                    if image_url:
                        rec["image_url"] = image_url
                
                await asyncio.gather(*(fetch_and_assign(rec) for rec in missing_images))
                # Update cache with new images
                set_cache(cache_key, cached, ttl_seconds=CACHE_TTL)
                
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
        
        # Concurrently fetch images for each recommendation destination
        if "recommendations" in data:
            async def fetch_and_assign(rec):
                image_url = await pexels_service.search_image(rec.get("destination", ""))
                if image_url:
                    rec["image_url"] = image_url
            
            await asyncio.gather(*(fetch_and_assign(rec) for rec in data["recommendations"]))

        set_cache(cache_key, data, ttl_seconds=CACHE_TTL)
        return data
    except Exception as e:
        logger.error(f"Failed to generate AI recommendations: {e}. Returning fallback data.")
        # Fallback high-quality mock data to prevent UI crash
        fallback = {
            "recommendations": [
                {
                    "destination": "Đà Lạt",
                    "highlight": "Thành phố ngàn hoa với khí hậu mát mẻ quanh năm.",
                    "best_for": "Nghỉ dưỡng, Cặp đôi",
                    "estimated_budget": "Trung bình",
                    "best_season": "Quanh năm",
                    "why_recommended": "Phù hợp với sở thích văn hóa và khí hậu mùa " + season,
                    "image_url": "https://images.pexels.com/photos/1329510/pexels-photo-1329510.jpeg"
                },
                {
                    "destination": "Hội An",
                    "highlight": "Phố cổ lung linh ánh đèn lồng và kiến trúc di sản.",
                    "best_for": "Văn hóa, Ẩm thực",
                    "estimated_budget": "Thấp - Trung bình",
                    "best_season": "Mùa xuân, Mùa hè",
                    "why_recommended": "Điểm đến văn hóa hàng đầu Việt Nam.",
                    "image_url": "https://images.pexels.com/photos/5409664/pexels-photo-5409664.jpeg"
                },
                {
                    "destination": "Sapa",
                    "highlight": "Ruộng bậc thang hùng vĩ và văn hóa dân tộc độc đáo.",
                    "best_for": "Khám phá, Thiên nhiên",
                    "estimated_budget": "Trung bình",
                    "best_season": "Mùa thu, Mùa xuân",
                    "why_recommended": "Cảnh quan thiên nhiên tuyệt đẹp và bản sắc văn hóa.",
                    "image_url": "https://images.pexels.com/photos/3356054/pexels-photo-3356054.jpeg"
                }
            ]
        }
        return fallback
