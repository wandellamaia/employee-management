export const EmployeeRole = {
    Employee: 1,
    Leader: 2,
    Director: 3
} as const;

export type EmployeeRole = typeof EmployeeRole[keyof typeof EmployeeRole];

export interface EmployeePhone {
    id?: number;
    employeeId?: number;
    phoneNumber: string;
    type?: string;
}

export interface Employee {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    documentNumber: string;
    role: EmployeeRole;
    managerId?: number;
    manager?: Employee;
    dateOfBirth: string;
    phones: EmployeePhone[];
}

export interface EmployeeCreateDto {
    firstName: string;
    lastName: string;
    email: string;
    documentNumber: string;
    password?: string;
    role: EmployeeRole;
    managerId?: number;
    dateOfBirth: string;
    phones: { phoneNumber: string; type?: string }[];
}

export interface EmployeeUpdateDto extends Omit<EmployeeCreateDto, 'password'> {
    password?: string;
}


export interface LoginDto {
    email: string;
    password: string;
}

export interface AuthResponse {
    token: string;
    employeeId: number;
    role: string; // Received as string from backend, needs to be handled
}
