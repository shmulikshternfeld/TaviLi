export interface UserProfile {
  id: string;
  email: string;
  name: string;
  profileImageUrl?: string;
  phoneNumber?: string;
  roles: string[];
}

export interface AuthResponse {
  user: UserProfile;
  token: string;
}
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest extends LoginRequest {
  name: string;
  email: string;
  password: string;
  wantsToBeClient: boolean;
  wantsToBeCourier: boolean;
}