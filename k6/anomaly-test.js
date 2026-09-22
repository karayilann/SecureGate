import http from 'k6/http';
import { check, sleep } from 'k6';

// Anomaly proof: hit ONE key from many distinct spoofed IPs (X-Forwarded-For). Once the
// distinct-IP count crosses the configured threshold, the background worker suspends the
// key mid-run and subsequent requests flip from 200 to 401.
//   BASE_URL  target origin (default http://localhost:5212)
//   API_KEY   a Pro or Enterprise key value works best (fewer 429s dilute the signal)
// For a fast demo, lower appsettings "Anomaly" (e.g. threshold 5, window 1, interval 15).
// Run: k6 run -e API_KEY=<key> k6/anomaly-test.js

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5212';
const API_KEY = __ENV.API_KEY;

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

  check(res, {
    'active (200)': (r) => r.status === 200,
    'rate-limited (429)': (r) => r.status === 429,
    'suspended (401)': (r) => r.status === 401,
  });

  sleep(1);
}
