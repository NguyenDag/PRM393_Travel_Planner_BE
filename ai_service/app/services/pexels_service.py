import httpx
import logging
from typing import Optional
from app.core.config import settings

logger = logging.getLogger(__name__)

async def search_image(query: str) -> Optional[str]:
    """
    Calls Pexels API to find a relevant image for the given query.
    Returns the URL of the medium-sized image or None if not found/error.
    """
    if not settings.PEXELS_API_KEY or settings.PEXELS_API_KEY == "<FILL_YOUR_PEXELS_API_KEY_HERE>":
        logger.warning("Pexels API key not configured. Skipping image fetch.")
        return None

    url = "https://api.pexels.com/v1/search"
    headers = {
        "Authorization": settings.PEXELS_API_KEY
    }
    params = {
        "query": query,
        "per_page": 1
    }

    try:
        async with httpx.AsyncClient() as client:
            response = await client.get(url, headers=headers, params=params, timeout=5.0)
            if response.status_code == 200:
                data = response.json()
                photos = data.get("photos", [])
                if photos:
                    # Return landscape or medium image
                    return photos[0].get("src", {}).get("landscape") or photos[0].get("src", {}).get("medium")
            else:
                logger.error(f"Pexels API error {response.status_code}: {response.text}")
    except Exception as e:
        logger.error(f"Failed to fetch image from Pexels: {e}")

    return None
