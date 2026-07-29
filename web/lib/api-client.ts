import { auth } from "@/lib/auth";
import type { ApiProblem } from "@/lib/types";

const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5080";

export class ApiError extends Error {
  status: number;
  problem?: ApiProblem;

  constructor(status: number, message: string, problem?: ApiProblem) {
    super(message);
    this.status = status;
    this.problem = problem;
  }
}

type RequestOptions = Omit<RequestInit, "body"> & { body?: unknown };

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const session = await auth();
  const token = session?.accessToken;

  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
    cache: "no-store",
  });

  if (!res.ok) {
    let problem: ApiProblem | undefined;
    try {
      problem = await res.json();
    } catch {
      // non-JSON error body, ignore
    }
    throw new ApiError(res.status, problem?.detail ?? res.statusText, problem);
  }

  if (res.status === 204) return undefined as T;

  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

// Server-side authenticated client - use from Server Components / Server Actions only.
export const apiClient = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body?: unknown) => request<T>(path, { method: "POST", body }),
  put: <T>(path: string, body?: unknown) => request<T>(path, { method: "PUT", body }),
  patch: <T>(path: string, body?: unknown) => request<T>(path, { method: "PATCH", body }),
  del: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};

// Public, unauthenticated fetch for CMS-driven marketing pages - supports ISR via `revalidate`.
export async function publicGet<T>(path: string, revalidateSeconds = 60): Promise<T | null> {
  const res = await fetch(`${API_BASE_URL}${path}`, { next: { revalidate: revalidateSeconds } });
  if (res.status === 404) return null;
  if (!res.ok) throw new ApiError(res.status, res.statusText);
  return res.json() as Promise<T>;
}
