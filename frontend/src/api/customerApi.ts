import type { CreateCustomerRequest, CustomerDto, ApiError } from '../types/Customer';

const BASE = import.meta.env.VITE_API_BASE_URL ?? '/api';

async function handle<T>(res: Response): Promise<T> {
  if (res.ok) return (await res.json()) as T;
  let detail = res.statusText;
  let title = 'Request failed';
  try {
    const body = await res.json();
    title = body.title ?? title;
    detail = body.detail ?? detail;
  } catch { /* not JSON */ }
  const err: ApiError = { status: res.status, title, detail };
  throw err;
}

export async function createCustomer(req: CreateCustomerRequest): Promise<CustomerDto> {
  const res = await fetch(`${BASE}/customers`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(req),
  });
  return handle<CustomerDto>(res);
}

export async function listCustomers(): Promise<CustomerDto[]> {
  return handle<CustomerDto[]>(await fetch(`${BASE}/customers`));
}

export async function getCustomer(id: string): Promise<CustomerDto> {
  return handle<CustomerDto>(await fetch(`${BASE}/customers/${id}`));
}
