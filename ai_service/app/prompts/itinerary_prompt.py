def build_itinerary_prompt(destination: str, days: int, budget: str, style: str, notes: str = "") -> str:
    notes_section = f"\n    - Ghi chú đặc biệt: {notes}" if notes else ""

    return f"""
Bạn là chuyên gia du lịch Việt Nam với 10 năm kinh nghiệm. 
Hãy tạo lịch trình {days} ngày tại {destination} thật chi tiết và thực tế.

Thông tin chuyến đi:
    - Điểm đến: {destination}
    - Số ngày: {days} ngày
    - Phong cách: {style} (Adventure/Relaxing/Cultural)
    - Ngân sách: {budget} (low = tiết kiệm / medium = trung bình / high = cao cấp){notes_section}

Yêu cầu:
    - Mỗi ngày có 3-5 hoạt động hợp lý về mặt địa lý (tránh di chuyển quá xa)
    - Thời gian hoạt động thực tế (không xếp quá dày)
    - Chi phí ước tính phù hợp với mức ngân sách {budget}
    - Ưu tiên các địa điểm thực tế, nổi tiếng tại {destination}

Trả về JSON theo ĐÚNG cấu trúc sau, KHÔNG thêm bất kỳ text hoặc markdown nào khác:
{{
  "trip_name": "Tên gợi cảm cho chuyến đi",
  "destination": "{destination}",
  "days": [
    {{
      "day_number": 1,
      "activities": [
        {{
          "time": "08:00",
          "title": "Tên hoạt động ngắn gọn",
          "location": "Tên địa điểm cụ thể",
          "description": "Mô tả 1-2 câu về hoạt động",
          "estimated_cost": "Ví dụ: 50.000đ - 100.000đ hoặc Miễn phí"
        }}
      ]
    }}
  ]
}}
""".strip()
