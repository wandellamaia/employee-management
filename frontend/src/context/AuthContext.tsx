import React, { createContext, useContext, useState } from 'react';
import type { AuthResponse, EmployeeRole } from '../types';

interface AuthContextType {
    token: string | null;
    user: { id: number; role: EmployeeRole } | null;
    login: (authData: AuthResponse) => void;
    logout: () => void;
    isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
    const [user, setUser] = useState<{ id: number; role: EmployeeRole } | null>(
        JSON.parse(localStorage.getItem('user') || 'null')
    );

    const login = (authData: AuthResponse) => {
        localStorage.setItem('token', authData.token);
        const userData = { id: authData.employeeId, role: Number(authData.role) as EmployeeRole };
        localStorage.setItem('user', JSON.stringify(userData));
        setToken(authData.token);
        setUser(userData);
    };

    const logout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        setToken(null);
        setUser(null);
    };

    const isAuthenticated = !!token;

    return (
        <AuthContext.Provider value={{ token, user, login, logout, isAuthenticated }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};
