import http from 'k6/http';
import { check } from 'k6';

// Rate-limit proof: fire a burst on ONE Free-plan key (limit 10/min) and watch the
// first ~10 requests return 200 and the rest 429.
//   BASE_URL  target origin (default http://localhost:5212)
//   API_KEY   a Free-plan key value (required)
// Run: k6 run -e API_KEY=<key> k6/rate-limit-test.js

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5212';
const API_KEY = __ENV.API_KEY;

export const options = {
  scenarios: {
    burst: { executor: 'per-vu-iterations', vus: 1, iterations: 20, maxDuration: '30s' },
  },
};

export default function () {
  const res = http.get(`${BASE_URL}/proxy?resource=report`, {
    headers: { 'X-Api-Key': API_KEY },
  });

  check(res, {
    'allowed (200)': (r) => r.status === 200,
    'rate-limited (429)': (r) => r.status === 429,
  });
}
