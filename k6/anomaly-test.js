import http from 'k6/http';
import { sleep } from 'k6';
import { Counter } from 'k6/metrics';

// Anomaly proof: hit ONE key from many distinct spoofed IPs (X-Forwarded-For). Once the
// distinct-IP count crosses the configured threshold, the background worker suspends the
// key mid-run and subsequent requests flip from 200 to 401.
//   BASE_URL  target origin (default http://localhost:5212)
//   API_KEY   the key value to abuse (required)
// For a fast demo, lower appsettings "Anomaly" (e.g. threshold 5, window 1, interval 15).
// Run: k6 run -e API_KEY=<key> k6/anomaly-test.js

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5212';
const API_KEY = __ENV.API_KEY;

// Treat 200/401/429 as expected so http_req_failed reflects real errors only.
http.setResponseCallback(http.expectedStatuses(200, 401, 429));

const allowed = new Counter('status_200_allowed');
const rateLimited = new Counter('status_429_rate_limited');
const suspended = new Counter('status_401_suspended');

export const options = {
  scenarios: {
    distinct_ips: { executor: 'per-vu-iterations', vus: 1, iterations: 90, maxDuration: '3m' },
  },
};

export default function () {
  const spoofedIp = `203.0.113.${__ITER % 256}`;

  const res = http.get(`${BASE_URL}/proxy?resource=report`, {
    headers: { 'X-Api-Key': API_KEY, 'X-Forwarded-For': spoofedIp },
  });

  if (res.status === 200) allowed.add(1);
  else if (res.status === 429) rateLimited.add(1);
  else if (res.status === 401) suspended.add(1);

  sleep(1);
}
