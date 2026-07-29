import NextAuth from "next-auth";
import Credentials from "next-auth/providers/credentials";
import Google from "next-auth/providers/google";

const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5080";

type ApiTokens = {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
};

type ApiMe = {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  memberId?: string;
};

async function fetchMe(accessToken: string): Promise<ApiMe | null> {
  const res = await fetch(`${API_BASE_URL}/api/v1/auth/me`, {
    headers: { Authorization: `Bearer ${accessToken}` },
    cache: "no-store",
  });
  if (!res.ok) return null;
  return res.json();
}

async function refreshAccessToken(refreshToken: string): Promise<ApiTokens | null> {
  const res = await fetch(`${API_BASE_URL}/api/v1/auth/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
    cache: "no-store",
  });
  if (!res.ok) return null;
  return res.json();
}

export const { handlers, auth, signIn, signOut } = NextAuth({
  session: { strategy: "jwt" },
  secret: process.env.NEXTAUTH_SECRET,
  trustHost: true,
  pages: {
    signIn: "/login",
  },
  providers: [
    Credentials({
      credentials: {
        email: { label: "Email", type: "email" },
        password: { label: "Password", type: "password" },
      },
      authorize: async (credentials) => {
        const email = credentials?.email as string | undefined;
        const password = credentials?.password as string | undefined;
        if (!email || !password) return null;

        const res = await fetch(`${API_BASE_URL}/api/v1/auth/login`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ email, password }),
          cache: "no-store",
        });
        if (!res.ok) return null;

        const tokens = (await res.json()) as ApiTokens;
        const me = await fetchMe(tokens.accessToken);
        if (!me) return null;

        return {
          id: me.userId,
          email: me.email,
          name: `${me.firstName} ${me.lastName}`.trim(),
          accessToken: tokens.accessToken,
          refreshToken: tokens.refreshToken,
          accessTokenExpiresAtUtc: tokens.accessTokenExpiresAtUtc,
          roles: me.roles,
          memberId: me.memberId,
        };
      },
    }),
    Google({
      clientId: process.env.GOOGLE_CLIENT_ID,
      clientSecret: process.env.GOOGLE_CLIENT_SECRET,
    }),
  ],
  callbacks: {
    async jwt({ token, user, account }) {
      const t = token as CrewFlowToken;
      const u = user as CrewFlowUser | undefined;

      // Credentials sign-in: authorize() already returned our API's token pair + profile.
      if (u) {
        t.accessToken = u.accessToken;
        t.refreshToken = u.refreshToken;
        t.accessTokenExpires = u.accessTokenExpiresAtUtc;
        t.roles = u.roles;
        t.memberId = u.memberId;
      }

      // Google sign-in: exchange the verified Google id_token for our own API JWT pair via
      // POST /auth/external, so downstream code always deals with a single token contract
      // regardless of how the user signed in.
      if (account?.provider === "google" && account.id_token) {
        const res = await fetch(`${API_BASE_URL}/api/v1/auth/external`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ provider: "Google", idToken: account.id_token }),
          cache: "no-store",
        });

        if (res.ok) {
          const tokens = (await res.json()) as ApiTokens;
          const me = await fetchMe(tokens.accessToken);
          t.accessToken = tokens.accessToken;
          t.refreshToken = tokens.refreshToken;
          t.accessTokenExpires = tokens.accessTokenExpiresAtUtc;
          t.roles = me?.roles ?? [];
          t.memberId = me?.memberId;
        }
      }

      const expiresAt = t.accessTokenExpires ? new Date(t.accessTokenExpires).getTime() : 0;
      if (t.refreshToken && expiresAt && Date.now() > expiresAt - 60_000) {
        const refreshed = await refreshAccessToken(t.refreshToken);
        if (refreshed) {
          t.accessToken = refreshed.accessToken;
          t.refreshToken = refreshed.refreshToken;
          t.accessTokenExpires = refreshed.accessTokenExpiresAtUtc;
        }
      }

      return t;
    },
    async session({ session, token }) {
      const t = token as CrewFlowToken;
      const s = session as typeof session & { accessToken?: string; roles?: string[]; memberId?: string };
      s.accessToken = t.accessToken;
      s.roles = t.roles ?? [];
      s.memberId = t.memberId;
      return s;
    },
  },
});

type CrewFlowToken = {
  accessToken?: string;
  refreshToken?: string;
  accessTokenExpires?: string;
  roles?: string[];
  memberId?: string;
  [key: string]: unknown;
};

type CrewFlowUser = {
  accessToken?: string;
  refreshToken?: string;
  accessTokenExpiresAtUtc?: string;
  roles?: string[];
  memberId?: string;
};
