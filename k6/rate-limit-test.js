import http from 'k6/http';
import { Counter } from 'k6/metrics';

// Rate-limit proof: fire a burst on ONE Free-plan key (limit 10/min) and watch the
// first ~10 requests return 200 and the rest 429.
//   BASE_URL  target origin (default http://localhost:5212)
//   API_KEY   a Free-plan key value (required)
// Run: k6 run -e API_KEY=<key> k6/rate-limit-test.js

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5212';
const API_KEY = __ENV.API_KEY;

// Treat 200/429 as expected so http_req_failed reflects real errors only.
http.setResponseCallback(http.expectedStatuses(200, 429));

const allowed = new Counter('status_200_allowed');
const rateLimited = new Counter('status_429_rate_limited');

export const options = {
  scenarios: {
    burst: { executor: 'per-vu-iterations', vus: 1, iterations: 20, maxDuration: '30s' },
  },
};

export default function () {
  const res = http.get(`${BASE_URL}/proxy?resource=report`, {
    headers: { 'X-Api-Key': API_KEY },
  });

  if (res.status === 200) allowed.add(1);
  else if (res.status === 429) rateLimited.add(1);
}
