import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import apiClient from '../api/apiClient';
import type { Employee, EmployeeCreateDto } from '../types';
import { EmployeeRole } from '../types';
import {
    ArrowLeft,
    Save,
    Plus,
    Trash2,
    User,
    Mail,
    FileText,
    Calendar,
    Phone,
    Briefcase,
    AlertTriangle
} from 'lucide-react';

const EmployeeFormPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const isEdit = !!id;
    const navigate = useNavigate();
    const { user } = useAuth();

    const [formData, setFormData] = useState<EmployeeCreateDto>({
        firstName: '',
        lastName: '',
        email: '',
        documentNumber: '',
        password: '',
        role: EmployeeRole.Employee,
        dateOfBirth: '',
        phones: [{ phoneNumber: '', type: 'Mobile' }]
    });

    const [employees, setEmployees] = useState<Employee[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const empResponse = await apiClient.get('/Employees');
                setEmployees(empResponse.data);

                if (isEdit) {
                    const response = await apiClient.get(`/Employees/${id}`);
                    const emp = response.data;
                    setFormData({
                        firstName: emp.firstName,
                        lastName: emp.lastName,
                        email: emp.email,
                        documentNumber: emp.documentNumber,
                        role: emp.role,
                        managerId: emp.managerId,
                        dateOfBirth: emp.dateOfBirth.split('T')[0],
                        phones: emp.phones.map((p: any) => ({ phoneNumber: p.phoneNumber, type: p.type }))
                    });
                }
            } catch (err) {
                console.error('Failed to fetch data', err);
            }
        };
        fetchData();
    }, [id, isEdit]);

    const handlePhoneChange = (index: number, value: string) => {
        const newPhones = [...formData.phones];
        newPhones[index].phoneNumber = value;
        setFormData({ ...formData, phones: newPhones });
    };

    const addPhone = () => {
        setFormData({
            ...formData,
            phones: [...formData.phones, { phoneNumber: '', type: 'Mobile' }]
        });
    };

    const removePhone = (index: number) => {
        if (formData.phones.length === 1) return;
        setFormData({
            ...formData,
            phones: formData.phones.filter((_, i) => i !== index)
        });
    };

    const validateAge = (dob: string) => {
        const birthDate = new Date(dob);
        const today = new Date();
        let age = today.getFullYear() - birthDate.getFullYear();
        const monthDiff = today.getMonth() - birthDate.getMonth();
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
            age--;
        }
        return age >= 18;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        // Age validation
        if (!validateAge(formData.dateOfBirth)) {
            setError('Employee must be at least 18 years old.');
            return;
        }

        // Role check (Frontend)
        if (user && formData.role > user.role) {
            setError('You cannot create an employee with a higher role than yours.');
            return;
        }

        setIsLoading(true);
        try {
            if (isEdit) {
                await apiClient.put(`/Employees/${id}`, formData);
            } else {
                await apiClient.post('/Employees', formData);
            }
            navigate('/');
        } catch (err: any) {
            setError(err.response?.data || 'Failed to save employee.');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="container" style={{ paddingTop: '40px', paddingBottom: '80px' }}>
            <button
                onClick={() => navigate('/')}
                style={{
                    background: 'transparent',
                    color: 'var(--text-secondary)',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px',
                    marginBottom: '24px',
                    padding: '0'
                }}
            >
                <ArrowLeft size={18} />
                Back to Dashboard
            </button>

            <div style={{ maxWidth: '800px', margin: '0 auto' }}>
                <h1 style={{ marginBottom: '32px' }}>{isEdit ? 'Edit Employee' : 'Create New Employee'}</h1>

                <form onSubmit={handleSubmit} className="glass-card">
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '24px', marginBottom: '24px' }}>
                        <div>
                            <label>First Name</label>
                            <div style={{ position: 'relative' }}>
                                <User size={18} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-secondary)' }} />
                                <input
                                    type="text"
                                    required
                                    value={formData.firstName}
                                    onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                                    style={{ paddingLeft: '40px' }}
                                />
                            </div>
                        </div>
                        <div>
                            <label>Last Name</label>
                            <div style={{ position: 'relative' }}>
                                <User size={18} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-secondary)' }} />
                                <input
                                    type="text"
                                    required
                                    value={formData.lastName}
                                    onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                                    style={{ paddingLeft: '40px' }}
                                />
                            </div>
                        </div>
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '24px', marginBottom: '24px' }}>
                        <div>
                            <label>Email Address</label>
                            <div style={{ position: 'relative' }}>
                                <Mail size={18} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-secondary)' }} />
                                <input
                                    type="email"
                                    required
                                    value={formData.email}
                                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                                    style={{ paddingLeft: '40px' }}
                                />
                            </div>
                        </div>
                        <div>
                            <label>Document Number (CPF/ID)</label>
                            <div style={{ position: 'relative' }}>
                                <FileText size={18} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-secondary)' }} />
                                <input
                                    type="text"
                                    required
                                    value={formData.documentNumber}
                                    onChange={(e) => setFormData({ ...formData, documentNumber: e.target.value })}
                                    style={{ paddingLeft: '40px' }}
                                />
                            </div>
                        </div>
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '24px', marginBottom: '24px' }}>
                        {!isEdit && (
                            <div>
                                <label>Password</label>
                                <input
                                    type="password"
                                    required
                                    value={formData.password}
                                    onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                                />
                            </div>
                        )}
                        <div>
                            <label>Date of Birth</label>
                            <div style={{ position: 'relative' }}>
                                <Calendar size={18} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-secondary)' }} />
                                <input
                                    type="date"
                                    required
                                    value={formData.dateOfBirth}
                                    onChange={(e) => setFormData({ ...formData, dateOfBirth: e.target.value })}
                                    style={{ paddingLeft: '40px' }}
                                />
                            </div>
                        </div>
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '24px', marginBottom: '32px' }}>
                        <div>
                            <label>Role</label>
                            <div style={{ position: 'relative' }}>
                                <Briefcase size={18} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-secondary)' }} />
                                <select
                                    value={formData.role}
                                    onChange={(e) => setFormData({ ...formData, role: Number(e.target.value) as EmployeeRole })}
                                    style={{ paddingLeft: '40px' }}
                                >
                                    <option value={EmployeeRole.Employee}>Employee</option>
                                    <option value={EmployeeRole.Leader}>Leader</option>
                                    <option value={EmployeeRole.Director}>Director</option>
                                </select>
                            </div>
                        </div>
                        <div>
                            <label>Manager</label>
                            <select
                                value={formData.managerId || ''}
                                onChange={(e) => setFormData({ ...formData, managerId: e.target.value ? Number(e.target.value) : undefined })}
                            >
                                <option value="">No Manager</option>
                                {employees.filter(e => e.id !== Number(id)).map(emp => (
                                    <option key={emp.id} value={emp.id}>
                                        {emp.firstName} {emp.lastName} ({Object.keys(EmployeeRole).find(key => (EmployeeRole as any)[key] === emp.role)})
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div style={{ marginBottom: '32px' }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
                            <label style={{ marginBottom: 0 }}>Phone Numbers</label>
                            <button
                                type="button"
                                onClick={addPhone}
                                style={{ background: 'var(--glass-bg)', padding: '6px 12px', fontSize: '0.85rem' }}
                            >
                                <Plus size={14} style={{ marginRight: '4px' }} /> Add Phone
                            </button>
                        </div>
                        {formData.phones.map((phone, index) => (
                            <div key={index} style={{ display: 'flex', gap: '12px', marginBottom: '12px' }}>
                                <div style={{ position: 'relative', flex: 1 }}>
                                    <Phone size={18} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-secondary)' }} />
                                    <input
                                        type="text"
                                        required
                                        placeholder="Phone number"
                                        value={phone.phoneNumber}
                                        onChange={(e) => handlePhoneChange(index, e.target.value)}
                                        style={{ paddingLeft: '40px' }}
                                    />
                                </div>
                                {formData.phones.length > 1 && (
                                    <button
                                        type="button"
                                        onClick={() => removePhone(index)}
                                        style={{ background: 'rgba(239, 68, 68, 0.1)', color: 'var(--error)', padding: '12px' }}
                                    >
                                        <Trash2 size={18} />
                                    </button>
                                )}
                            </div>
                        ))}
                    </div>

                    {error && (
                        <div style={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: '8px',
                            color: 'var(--error)',
                            background: 'rgba(239, 68, 68, 0.1)',
                            padding: '12px',
                            borderRadius: '8px',
                            marginBottom: '24px'
                        }}>
                            <AlertTriangle size={18} />
                            <span>{error}</span>
                        </div>
                    )}

                    <div style={{ display: 'flex', gap: '12px', justifyContent: 'flex-end' }}>
                        <button
                            type="button"
                            onClick={() => navigate('/')}
                            style={{ background: 'var(--glass-bg)', color: 'var(--text-primary)' }}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="btn-primary"
                            disabled={isLoading}
                            style={{ display: 'flex', alignItems: 'center', gap: '8px' }}
                        >
                            <Save size={18} />
                            {isLoading ? 'Saving...' : 'Save Employee'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default EmployeeFormPage;
