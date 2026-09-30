import http from 'k6/http';
import { check } from 'k6';

export const options = { vus: 50, duration: '30s' };   // 50 simulated users for 30 seconds
const BASE = __ENV.BASE || 'http://localhost:8080';

export function setup() {
  const json = { 'Content-Type': 'application/json' };
  const creds = JSON.stringify({ email: 'load@test.local', password: 'Load!Test2026' });
  http.post(`${BASE}/auth/register`, creds, { headers: json });   // fine if it already exists
  const token = http.post(`${BASE}/auth/login`, creds, { headers: json }).json('token');
  const auth = { headers: { ...json, Authorization: `Bearer ${token}` } };

  // Seed 50 items once, so the response is a realistic size
  if (http.get(`${BASE}/pantry`, auth).json().length === 0) {
    const names = ['eggs', 'rice', 'spinach', 'onion', 'cheddar', 'tortillas', 'milk', 'butter', 'flour', 'garlic'];
    for (let i = 0; i < 50; i++) {
      http.post(`${BASE}/pantry`, JSON.stringify({ ingredient: names[i % 10], quantity: 1, unit: 'count' }), auth);
    }
  }
  return { token };
}

export default function (data) {
  const res = http.get(`${BASE}/pantry`, { headers: { Authorization: `Bearer ${data.token}` } });
  check(res, { 'status is 200': r => r.status === 200 });
}