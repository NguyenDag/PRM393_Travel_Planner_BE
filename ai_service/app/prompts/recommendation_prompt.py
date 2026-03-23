def build_recommendation_prompt(
    interests: list[str],
    past_destinations: list[str],
    season: str,
    budget: str,
) -> str:
    interests_str = ", ".join(interests) if interests else "đa dạng"
    past_str = ", ".join(past_destinations) if past_destinations else "chưa có"

    return f"""
Bạn là chuyên gia tư vấn du lịch Việt Nam. Hãy gợi ý 5 điểm đến phù hợp cho người dùng.

Thông tin người dùng:
    - Sở thích du lịch: {interests_str}
    - Đã từng đến: {past_str}
    - Mùa du lịch: {season}
    - Ngân sách: {budget}

Yêu cầu:
    - Ưu tiên điểm đến mới (chưa từng đến nếu có)
    - Phù hợp với thời tiết mùa {season}
    - Phù hợp ngân sách {budget}
    - Mỗi gợi ý kèm lý do ngắn gọn

Trả về JSON theo ĐÚNG cấu trúc sau, KHÔNG thêm text hoặc markdown nào khác:
{{
  "recommendations": [
    {{
      "destination": "Tên điểm đến",
      "province": "Tỉnh/Thành phố",
      "highlight": "1 câu nổi bật nhất",
      "best_for": "Loại hình du lịch phù hợp",
      "estimated_budget": "Ngân sách ước tính cho 3 ngày",
      "best_season": "Mùa đẹp nhất trong năm",
      "why_recommended": "Lý do phù hợp với người dùng này"
    }}
  ]
}}
""".strip()


def build_chat_system_prompt(trip_context: dict | None) -> str:
    base = (
        "Bạn là trợ lý du lịch thông minh của ứng dụng Du Xuan Planner. "
        "Bạn giúp người dùng giải đáp thắc mắc về du lịch, gợi ý địa điểm, "
        "nhà hàng, khách sạn và hỗ trợ xử lý tình huống phát sinh trong chuyến đi. "
        "Trả lời ngắn gọn, thân thiện, bằng tiếng Việt."
    )

    if not trip_context:
        return base

    context_parts = []
    if trip_context.get("destination"):
        context_parts.append(f"Điểm đến hiện tại: {trip_context['destination']}")
    if trip_context.get("current_day"):
        context_parts.append(f"Ngày du lịch thứ: {trip_context['current_day']}")
    if trip_context.get("current_activity"):
        context_parts.append(f"Hoạt động hiện tại: {trip_context['current_activity']}")
    if trip_context.get("current_time"):
        context_parts.append(f"Thời gian: {trip_context['current_time']}")

    if context_parts:
        return base + "\n\nNgữ cảnh chuyến đi của người dùng:\n" + "\n".join(f"- {p}" for p in context_parts)

    return base
