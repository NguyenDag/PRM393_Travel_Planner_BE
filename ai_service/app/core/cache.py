import json
import hashlib
import logging
from typing import Any, Optional

logger = logging.getLogger(__name__)

_redis_client = None
_redis_disabled = False
_in_memory_cache = {}


def _get_client():
    global _redis_client, _redis_disabled
    if _redis_disabled:
        return None
        
    if _redis_client is not None:
        return _redis_client

    from app.core.config import settings
    if not settings.REDIS_URL:
        return None

    try:
        import redis
        _redis_client = redis.from_url(settings.REDIS_URL, decode_responses=True)
        _redis_client.ping()
        logger.info("Redis connected successfully.")
    except Exception as e:
        logger.warning(f"Redis connection failed. Caching will use in-memory fallback. Error: {e}")
        _redis_client = None
        _redis_disabled = True

    return _redis_client


def make_cache_key(prefix: str, data: Any) -> str:
    payload = json.dumps(data, sort_keys=True, ensure_ascii=False)
    digest = hashlib.md5(payload.encode()).hexdigest()
    return f"{prefix}:{digest}"


def get_cache(key: str) -> Optional[dict]:
    client = _get_client()
    if client:
        try:
            value = client.get(key)
            return json.loads(value) if value else None
        except Exception as e:
            logger.warning(f"Redis get error: {e}, falling back to memory")

    import time
    if key in _in_memory_cache:
        val, expiry = _in_memory_cache[key]
        if expiry is None or time.time() < expiry:
            return val
        else:
            del _in_memory_cache[key]
    return None


def set_cache(key: str, value: dict, ttl_seconds: int = 3600) -> None:
    client = _get_client()
    if client:
        try:
            client.setex(key, ttl_seconds, json.dumps(value, ensure_ascii=False))
            return
        except Exception as e:
            logger.warning(f"Redis set error: {e}, falling back to memory")
            
    import time
    expiry = time.time() + ttl_seconds
    _in_memory_cache[key] = (value, expiry)
    
    # Simple memory cleanup to prevent unbounded growth
    if len(_in_memory_cache) > 1000:
        now = time.time()
        expired_keys = [k for k, v in _in_memory_cache.items() if v[1] is not None and v[1] < now]
        for k in expired_keys:
            del _in_memory_cache[k]
