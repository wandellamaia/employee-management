export const getErrorMessage = (error: any): string => {
    if (!error.response) {
        return 'Network error. Please check your connection.';
    }

    const data = error.response.data;

    // Handle string responses (e.g. BadRequest("message"))
    if (typeof data === 'string') {
        return data;
    }

    // Handle problem details object (ASP.NET Core standard)
    if (data && typeof data === 'object') {
        // If there are validation errors, pick the first one
        if (data.errors) {
            const firstErrorField = Object.keys(data.errors)[0];
            const fieldErrors = data.errors[firstErrorField];
            if (Array.isArray(fieldErrors) && fieldErrors.length > 0) {
                return fieldErrors[0];
            }
        }

        // Handle common message properties
        if (data.detail) return data.detail;
        if (data.message) return data.message;
        if (data.title) return data.title;
    }

    return 'An unexpected error occurred.';
};
