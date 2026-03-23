import json
import hashlib
import logging
from typing import Any, Optional

logger = logging.getLogger(__name__)

_redis_client = None


def _get_client():
    global _redis_client
    if _redis_client is not None:
        return _redis_client

    from app.core.config import settings
    if not settings.REDIS_URL:
        return None

    try:
        import redis
        _redis_client = redis.from_url(settings.REDIS_URL, decode_responses=True)
        _redis_client.ping()
        logger.info("Redis connected.")
    except Exception as e:
        logger.warning(f"Redis unavailable, caching disabled: {e}")
        _redis_client = None

    return _redis_client


def make_cache_key(prefix: str, data: Any) -> str:
    payload = json.dumps(data, sort_keys=True, ensure_ascii=False)
    digest = hashlib.md5(payload.encode()).hexdigest()
    return f"{prefix}:{digest}"


def get_cache(key: str) -> Optional[dict]:
    client = _get_client()
    if not client:
        return None
    try:
        value = client.get(key)
        return json.loads(value) if value else None
    except Exception as e:
        logger.warning(f"Cache get error: {e}")
        return None


def set_cache(key: str, value: dict, ttl_seconds: int = 3600) -> None:
    client = _get_client()
    if not client:
        return
    try:
        client.setex(key, ttl_seconds, json.dumps(value, ensure_ascii=False))
    except Exception as e:
        logger.warning(f"Cache set error: {e}")
