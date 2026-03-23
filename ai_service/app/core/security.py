from fastapi import Security, HTTPException, status
from fastapi.security import APIKeyHeader
from app.core.config import settings

api_key_header = APIKeyHeader(name="X-Internal-Secret", auto_error=False)


async def verify_internal_token(api_key: str = Security(api_key_header)) -> str:
    """
    Verifies the shared internal secret between ASP.NET Core and this service.
    Bypassed in development mode to facilitate testing.
    """
    if settings.APP_ENV == "development":
        return api_key or "dev_token"

    if not api_key or api_key != settings.INTERNAL_SECRET:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Invalid or missing internal secret.",
        )
    return api_key
