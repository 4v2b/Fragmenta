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
        const data = await response.json();

        console.log('Response on login:', data);

        if (response.status == 400) {
            return { status: response.status, errors: data.errors }
        }

        if (response.ok) {
            return { status: response.status, data };
        }

        return { status: response.status }
    } catch (error) {
        console.error('Error:', error);
    }
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

        if (response.ok) {
            return { status: response.status, data };
        }

        return { status: response.status, errors: data.errors }

    } catch (error) {
        console.error('Error:', error);
    }
}

export async function refresh() {
    const baseUrl = import.meta.env.VITE_API_URL;
    const url = `${baseUrl}/refresh`;
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                "refreshToken": "string"
              }),
        });
        const data = await response.json();

        console.log('Response on refresh:', data);

        if (response.status == 400) {
            return { status: response.status, errors: data.errors }
        }

        if (response.ok) {
            return { status: response.status, data };
        }

        return { status: response.status }
    } catch (error) {
        console.error('Error:', error);
    }
}