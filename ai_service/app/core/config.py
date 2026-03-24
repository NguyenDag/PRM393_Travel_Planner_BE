from pydantic_settings import BaseSettings
from pydantic import ConfigDict
from typing import List


class Settings(BaseSettings):
    model_config = ConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",  # Ignore any extra fields in .env not declared here
    )

    OPENAI_API_KEY: str = ""
    INTERNAL_SECRET: str = "changeme"
    REDIS_URL: str = ""
    ALLOWED_ORIGINS: List[str] = ["http://localhost:3000", "http://localhost:5000"]
    APP_ENV: str = "development"

    # Server config (read from .env but not strictly required)
    HOST: str = "0.0.0.0"
    PORT: int = 8000
    ENVIRONMENT: str = "development"


settings = Settings()
