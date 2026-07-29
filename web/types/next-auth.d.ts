import type { DefaultSession } from "next-auth";

declare module "next-auth" {
  interface Session extends DefaultSession {
    accessToken?: string;
    roles?: string[];
    memberId?: string;
  }

  interface User {
    accessToken?: string;
    refreshToken?: string;
    accessTokenExpiresAtUtc?: string;
    roles?: string[];
    memberId?: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken?: string;
    refreshToken?: string;
    accessTokenExpires?: string;
    roles?: string[];
    memberId?: string;
  }
}
