// api/fetchClient.js
const BASE_URL = import.meta.env.VITE_API_URL;

// Frontend fetch client
async function fetchWithCookies(url, options = {}) {
    let response = await fetch(BASE_URL + url, {
        ...options,
        credentials: 'include'
    });

    if (response.status === 401) {
        // Try refresh
        const refreshResponse = await fetch(BASE_URL + '/refresh', {
            method: 'POST',
            credentials: 'include'
        });

        if (refreshResponse.ok) {
            // Retry original request - new JWT cookie will be included automatically
            response = await fetch(BASE_URL + url, {
                ...options,
                credentials: 'include'
            });
        } else {
            // Handle failed refresh (e.g., redirect to login)
            window.location.href = '/login';
        }
    }

    return response;
};

function logout() {
    console.log("You have been logged out")
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    window.location.href = "/login"; // Redirect to login page
}

async function fetchWithJwtBearer(url, options = {}) {

    const accessToken = localStorage.getItem("accessToken");

    if (!options.headers) options.headers = {};
    options.headers["Content-Type"] = "application/json";

    options.headers["Authorization"] = `Bearer ${accessToken}`;

    console.log("fetching data from ", url, "with data ", options)

    let response = await fetch(BASE_URL + url, options);

    if (response.status === 401) {
        try {
            console.log("Try to refresh token")
            await refreshToken();
            options.headers["Authorization"] = `Bearer ${localStorage.getItem("accessToken")}`;
            response = await fetch(BASE_URL + url, options);

            if (!response.ok) {
                console.log("Cannot refresh token")
                throw Error()
            }
            console.log(response, "from url ", url)
            return await response.json();

        } catch (error) {
            console.log(error)
            logout();
            return;
        }
    }

    if (response.status == 204) {
        return { status: response.status }
    }
    
    return await response.json()
};

export async function refreshToken() {
    const refreshToken = localStorage.getItem("refreshToken");

    console.log("Current refresh token", refreshToken)
    if (!refreshToken) return false;

    const response = await fetch(BASE_URL + "/refresh", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken })
    });

    console.log("Refresh response", response)

    if (!response.ok) {
        logout();
    }

    const data = await response.json();

    console.log("Access token", data)
    localStorage.setItem("accessToken", data);
}

export const api = {
    get: (url, workspaceId = null) => fetchWithJwtBearer(url, {
        headers: workspaceId ? {
            "X-Workspace-Id": workspaceId
        } : {}
    }),
    post: (url, data, workspaceId = null) => fetchWithJwtBearer(url, {
        headers: workspaceId ? {
            "X-Workspace-Id": workspaceId
        } : {},
        method: 'POST',
        body: JSON.stringify(data)
    }),
    put: (url, data, workspaceId = null) => fetchWithJwtBearer(url, {
        headers: workspaceId ? {
            "X-Workspace-Id": workspaceId
        } : {},
        method: 'PUT',
        body: JSON.stringify(data)
    }),
    delete:
        (url, workspaceId = null) => fetchWithJwtBearer(url, {
            headers: workspaceId ? {
                "X-Workspace-Id": workspaceId
            } : {},
            method: 'DELETE'
        })
    // Add other methods as needed
};