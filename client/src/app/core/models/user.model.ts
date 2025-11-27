export interface UserProfile {
  id: string;
  email: string;
  name: string;
  profileImageUrl?: string;
  roles: string[];
}

export interface AuthResponse {
  user: UserProfile;
  token: string;
}