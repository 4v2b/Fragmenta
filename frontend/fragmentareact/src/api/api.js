export async function login(email, password) {
    const baseUrl = import.meta.env.VITE_API_URL;
    const url = `${baseUrl}/login`;
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                "email": email,
                "password": password
            }),
        });

        console.log('Response on login:', response.status);
       

        const data = await response.json();

        console.log('Response content:', data);

        if (response.status == 400) {
            return { status: response.status, error: data.message ? data.message : Object.entries(data.errors).at(0)[1][0] }
        }

        if (response.ok) {
            localStorage.setItem("accessToken", data.accessToken);
            localStorage.setItem("refreshToken", data.refreshToken);
            return { status: response.status };
        }

        if (response.status === 423) {
            console.log(response)
            return { error: data.message, status: response.status, lockoutUntil: new Date(data.lockoutUntil).getTime()  }
        }

        return { status: response.status }

    } catch (error) {
        console.error('Error:', error);
    }
}

export function logout(){
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    window.location.href = "/login"; // Redirect to login page
}

export async function register(name, email, password) {
    const baseUrl = import.meta.env.VITE_API_URL;
    const url = `${baseUrl}/register`;
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                "email": email,
                "password": password,
                "name": name
            }),
        });


        const data = await response.json();

        console.log('Response on registration:', data);

        if (response.status == 400) {
            return { status: response.status, errors: data.errors, message: data.message ? data.message : Object.entries(data.errors).at(0)[1][0] }
        }

        if (response.ok) {
            localStorage.setItem("accessToken", data.accessToken);
            localStorage.setItem("refreshToken", data.refreshToken);
            return { status: response.status };
        }

        return { status: response.status}

    } catch (error) {
        console.error('Error:', error);
    }
}

export async function requestResetPassword(email, locale = "us-EN") {
    const baseUrl = import.meta.env.VITE_API_URL;
    const url = `${baseUrl}/forgot-password?email=${email}`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept-Language': locale
            }
        });

        if (response.ok) {
            return { status: response.status };
        }

        const data = await response.json();
        console.log('Response on reset password request:', data);

        if (response.status === 423) {
            console.log(response)
            return { message: data.message, status: response.status, lockoutUntil: new Date(data.lockoutUntil).getTime()  }
        }

        if (response.status == 400) {
            return { status: response.status, errors: data.errors, message: data.message ? data.message : Object.entries(data.errors).at(0)[1][0] }
        }

        return { status: response.status}

    } catch (error) {
        console.error('Error:', error);
    }
}

export async function resetPassword(token, userId, newPassword) {
    const baseUrl = import.meta.env.VITE_API_URL;
    const url = `${baseUrl}/reset-password`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                token, userId, newPassword
            })
        });

        if (response.ok) {
            return { status: response.status };
        }

        const data = await response.json();

        console.log('Response on reset password :', data);

        if (response.status == 400) {
            return { status: response.status, errors: data.errors, message: data.message ? data.message : Object.entries(data.errors).at(0)[1][0] }
        }

        return { status: response.status}

    } catch (error) {
        console.error('Error:', error);
    }
}