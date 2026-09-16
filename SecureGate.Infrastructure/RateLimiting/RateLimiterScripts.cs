namespace SecureGate.Infrastructure.RateLimiting;

internal static class RateLimiterScripts
{
    public const string SlidingWindow = @"
local key = KEYS[1]
local now = tonumber(ARGV[1])
local windowMs = tonumber(ARGV[2])
local limit = tonumber(ARGV[3])
local member = ARGV[4]

redis.call('ZREMRANGEBYSCORE', key, 0, now - windowMs)
local count = redis.call('ZCARD', key)

if count < limit then
    redis.call('ZADD', key, now, member)
    redis.call('PEXPIRE', key, windowMs)
    return {1, limit - count - 1, 0}
end

local oldest = redis.call('ZRANGE', key, 0, 0, 'WITHSCORES')
local retryMs = 0
if oldest[2] then
    retryMs = (tonumber(oldest[2]) + windowMs) - now
end
redis.call('PEXPIRE', key, windowMs)
return {0, 0, retryMs}
";
}
