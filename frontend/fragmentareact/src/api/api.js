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
            return { status: response.status, error: data.message }
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
    console.log({
        "email": email,
        "password": password,
        "name": name
    })
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