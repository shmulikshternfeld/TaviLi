// תואם ל-Backend Enums
export enum PackageSize {
  Small = 0,
  Medium = 1,
  Large = 2
}

export enum MissionStatus {
  Open = 0,
  Accepted = 1,
  InProgress_Pickup = 2,
  Collected = 3,
  InProgress_Delivery = 4,
  Completed = 5
}

export enum MissionRequestStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3
}

export interface MissionRequest {
  id: number;
  missionId: number;
  courierId: string;
  courierName: string;
  courierProfileImageUrl?: string;
  status: MissionRequestStatus;
  requestTime: Date; // ISO string from backend
}

// תואם ל-MissionSummaryDto ו-MissionDto
export interface Mission {
  id: number;
  pickupAddress: string;
  dropoffAddress: string;
  packageDescription: string;
  packageSize: PackageSize;
  offeredPrice: number;
  status: MissionStatus;
  creationTime: Date;
  creatorName: string;
  creatorUserId: string;
  creatorProfileImageUrl?: string;
  myRequestStatus?: MissionRequestStatus;
  pendingRequestsCount: number;
}