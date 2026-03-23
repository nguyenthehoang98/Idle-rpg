function error(code, message) {
    return {
        success: false,
        error: {
            code: code,
            message: message,
        }
    };
}