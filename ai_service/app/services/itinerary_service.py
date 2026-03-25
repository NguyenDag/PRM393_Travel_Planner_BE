import logging
from app.schemas.itinerary_schema import ItineraryRequest, ItineraryResponse
from app.prompts.itinerary_prompt import build_itinerary_prompt
from app.services import openai_service
from app.services import pexels_service
from app.core.cache import get_cache, set_cache, make_cache_key
import asyncio

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
    try:
        data = await openai_service.generate_json(prompt)
        # Verify
        result = ItineraryResponse(**data)
        
        # Fetch 1 image for the destination
        logger.info(f"Fetching cover image for destination: {request.destination}")
        image_url = await pexels_service.search_image(request.destination)
        if image_url:
            result.image_url = image_url
        
        # Save to cache as dict
        set_cache(cache_key, result.model_dump(), ttl_seconds=CACHE_TTL)
        return result
    except Exception as e:
        logger.error(f"Failed to generate AI itinerary: {e}. Returning fallback data.")
        # Minimal valid fallback
        limit_data = {
            "trip_name": "Khám phá Đà Lạt Mộng Mơ (Bản mẫu)",
            "destination": "Đà Lạt",
            "packing_list": [
                {"category": "Trang phục", "items": ["Áo khoác ấm", "Giày đi bộ", "Khăn choàng"]},
                {"category": "Giấy tờ", "items": ["CCCD/Hộ chiếu", "Bằng lái xe", "Vé máy bay/xe"]},
                {"category": "Khác", "items": ["Sạc dự phòng", "Kem chống nắng", "Ô/Dù"]}
            ],
            "days": [
                {
                    "day_number": 1,
                    "activities": [
                        {
                            "time": "08:00",
                            "title": "Ăn sáng Bánh mì xíu mại Hoàng Diệu",
                            "location": "26 Hoàng Diệu",
                            "description": "Thưởng thức món đặc sản nổi tiếng nhất Đà Lạt buổi sáng.",
                            "estimated_cost": "50.000 VNĐ"
                        },
                        {
                            "time": "10:00",
                            "title": "Tham quan Dinh I Bảo Đại",
                            "location": "Trần Quang Diệu",
                            "description": "Khám phá kiến trúc Pháp cổ kính và lịch sử triều Nguyễn.",
                            "estimated_cost": "90.000 VNĐ"
                        },
                        {
                            "time": "14:00",
                            "title": "Check-in Quảng trường Lâm Viên",
                            "location": "Trung tâm thành phố",
                            "description": "Chụp ảnh với nụ hoa Atiso và bông hoa Dã Quỳ khổng lồ.",
                            "estimated_cost": "Miễn phí"
                        }
                    ]
                },
                {
                    "day_number": 2,
                    "activities": [
                        {
                            "time": "09:00",
                            "title": "Tham quan Vườn hoa Cẩm Tú Cầu",
                            "location": "Trại Mát",
                            "description": "Ngắm nhìn cánh đồng hoa rộng lớn và chụp ảnh.",
                            "estimated_cost": "30.000 VNĐ"
                        },
                        {
                            "time": "15:00",
                            "title": "Chill tại quán Cà phê Túi Mơ To",
                            "location": "Sào Nam",
                            "description": "Ngắm hoàng hôn và tận hưởng không khí trong lành của Đà Lạt.",
                            "estimated_cost": "100.000 VNĐ"
                        }
                    ]
                },
                {
                    "day_number": 3,
                    "activities": [
                        {
                            "time": "08:30",
                            "title": "Mua sắm tại Chợ Đà Lạt",
                            "location": "Nguyễn Thị Minh Khai",
                            "description": "Mua đặc sản mứt, dâu tây làm quà cho người thân.",
                            "estimated_cost": "Tùy chọn"
                        },
                        {
                            "time": "11:00",
                            "title": "Thăm Thiền viện Trúc Lâm",
                            "location": "Núi Phượng Hoàng",
                            "description": "Tìm lại sự yên bình và ngắm cảnh hồ Tuyền Lâm từ trên cao.",
                            "estimated_cost": "Miễn phí"
                        }
                    ]
                }
            ]
        }
        return ItineraryResponse(**limit_data)
