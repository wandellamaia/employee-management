import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import apiClient from '../api/apiClient';
import type { Employee } from '../types';
import { EmployeeRole } from '../types';
import {
    UserPlus,
    LogOut,
    Edit2,
    Trash2,
    Search,
    Shield,
    ShieldCheck,
    ShieldAlert
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

const RoleBadge: React.FC<{ role: EmployeeRole }> = ({ role }) => {
    const config = {
        [EmployeeRole.Employee]: { icon: Shield, color: '#94a3b8', label: 'Employee' },
        [EmployeeRole.Leader]: { icon: ShieldCheck, color: '#38bdf8', label: 'Leader' },
        [EmployeeRole.Director]: { icon: ShieldAlert, color: '#6366f1', label: 'Director' }
    };

    const { icon: Icon, color, label } = config[role] || config[EmployeeRole.Employee];

    return (
        <div style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '6px',
            padding: '4px 12px',
            borderRadius: '20px',
            background: `${color}20`,
            color: color,
            fontSize: '0.8rem',
            fontWeight: 600
        }}>
            <Icon size={14} />
            {label}
        </div>
    );
};

const DashboardPage: React.FC = () => {
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [isLoading, setIsLoading] = useState(true);
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    const fetchEmployees = async () => {
        try {
            const response = await apiClient.get('/Employees');
            setEmployees(response.data);
        } catch (error) {
            console.error('Failed to fetch employees', error);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchEmployees();
    }, []);

    const handleDelete = async (id: number) => {
        if (!window.confirm('Are you sure you want to delete this employee?')) return;
        try {
            await apiClient.delete(`/Employees/${id}`);
            setEmployees(employees.filter(e => e.id !== id));
        } catch (error) {
            alert('Failed to delete employee');
        }
    };

    const filteredEmployees = employees.filter(e =>
        `${e.firstName} ${e.lastName}`.toLowerCase().includes(searchTerm.toLowerCase()) ||
        e.email.toLowerCase().includes(searchTerm.toLowerCase())
    );

    return (
        <div className="container" style={{ paddingTop: '40px', paddingBottom: '80px' }}>
            {/* Header */}
            <header style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '40px'
            }}>
                <div>
                    <h1 style={{ fontSize: '2rem', marginBottom: '8px' }}>Employee Directory</h1>
                    <p style={{ color: 'var(--text-secondary)' }}>Manage your team and their roles</p>
                </div>
                <div style={{ display: 'flex', gap: '12px' }}>
                    <button
                        onClick={() => navigate('/employee/new')}
                        className="btn-primary"
                        style={{ display: 'flex', alignItems: 'center', gap: '8px' }}
                    >
                        <UserPlus size={18} />
                        Add Employee
                    </button>
                    <button
                        onClick={logout}
                        style={{
                            background: 'var(--glass-bg)',
                            color: 'var(--text-primary)',
                            display: 'flex',
                            alignItems: 'center',
                            gap: '8px',
                            border: '1px solid var(--glass-border)'
                        }}
                    >
                        <LogOut size={18} />
                    </button>
                </div>
            </header>

            {/* Stats/Overview (Optional but adds premium feel) */}
            <div style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                gap: '20px',
                marginBottom: '40px'
            }}>
                <div className="glass-card" style={{ padding: '20px' }}>
                    <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '4px' }}>Total Employees</p>
                    <h2 style={{ fontSize: '1.8rem' }}>{employees.length}</h2>
                </div>
                <div className="glass-card" style={{ padding: '20px' }}>
                    <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '4px' }}>Active Projects</p>
                    <h2 style={{ fontSize: '1.8rem' }}>12</h2>
                </div>
                <div className="glass-card" style={{ padding: '20px' }}>
                    <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '4px' }}>Your Role</p>
                    <RoleBadge role={user?.role || EmployeeRole.Employee} />
                </div>
            </div>

            {/* Search Bar */}
            <div style={{ position: 'relative', marginBottom: '24px' }}>
                <Search size={20} style={{
                    position: 'absolute',
                    left: '16px',
                    top: '50%',
                    transform: 'translateY(-50%)',
                    color: 'var(--text-secondary)'
                }} />
                <input
                    type="text"
                    placeholder="Search by name or email..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    style={{ paddingLeft: '48px', height: '56px', fontSize: '1.1rem' }}
                />
            </div>

            {/* Employee List */}
            <div className="glass-card" style={{ padding: '0', overflow: 'hidden' }}>
                {isLoading ? (
                    <div style={{ padding: '40px', textAlign: 'center', color: 'var(--text-secondary)' }}>
                        Loading employees...
                    </div>
                ) : filteredEmployees.length === 0 ? (
                    <div style={{ padding: '40px', textAlign: 'center', color: 'var(--text-secondary)' }}>
                        No employees found.
                    </div>
                ) : (
                    <div style={{ overflowX: 'auto' }}>
                        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                            <thead>
                                <tr style={{ borderBottom: '1px solid var(--glass-border)' }}>
                                    <th style={{ padding: '16px 24px', color: 'var(--text-secondary)', fontWeight: 600 }}>Employee</th>
                                    <th style={{ padding: '16px 24px', color: 'var(--text-secondary)', fontWeight: 600 }}>Role</th>
                                    <th style={{ padding: '16px 24px', color: 'var(--text-secondary)', fontWeight: 600 }}>Document</th>
                                    <th style={{ padding: '16px 24px', color: 'var(--text-secondary)', fontWeight: 600 }}>Phones</th>
                                    <th style={{ padding: '16px 24px', color: 'var(--text-secondary)', fontWeight: 600, textAlign: 'right' }}>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                <AnimatePresence>
                                    {filteredEmployees.map((emp) => (
                                        <motion.tr
                                            key={emp.id}
                                            initial={{ opacity: 0 }}
                                            animate={{ opacity: 1 }}
                                            exit={{ opacity: 0 }}
                                            style={{ borderBottom: '1px solid var(--glass-border)', transition: 'background 0.2s' }}
                                        >
                                            <td style={{ padding: '16px 24px' }}>
                                                <div>
                                                    <p style={{ fontWeight: 600 }}>{emp.firstName} {emp.lastName}</p>
                                                    <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{emp.email}</p>
                                                </div>
                                            </td>
                                            <td style={{ padding: '16px 24px' }}>
                                                <RoleBadge role={emp.role} />
                                            </td>
                                            <td style={{ padding: '16px 24px', fontSize: '0.9rem' }}>
                                                {emp.documentNumber}
                                            </td>
                                            <td style={{ padding: '16px 24px', fontSize: '0.9rem' }}>
                                                {emp.phones.length > 0 ? emp.phones.map(p => p.phoneNumber).join(', ') : '-'}
                                            </td>
                                            <td style={{ padding: '16px 24px', textAlign: 'right' }}>
                                                <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
                                                    <button
                                                        onClick={() => navigate(`/employee/edit/${emp.id}`)}
                                                        style={{ background: 'transparent', color: 'var(--text-secondary)', padding: '8px' }}
                                                    >
                                                        <Edit2 size={18} />
                                                    </button>
                                                    <button
                                                        onClick={() => handleDelete(emp.id)}
                                                        style={{ background: 'transparent', color: 'var(--error)', padding: '8px' }}
                                                    >
                                                        <Trash2 size={18} />
                                                    </button>
                                                </div>
                                            </td>
                                        </motion.tr>
                                    ))}
                                </AnimatePresence>
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
};

export default DashboardPage;
