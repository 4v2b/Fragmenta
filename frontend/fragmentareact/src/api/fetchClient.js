const BASE_URL = import.meta.env.VITE_API_URL;

export async function refreshToken() {
    const refreshToken = localStorage.getItem("refreshToken");

    if (!refreshToken) return false;

    try {
        const response = await fetch(BASE_URL + "/refresh", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ refreshToken })
        });

        if (!response.ok) throw new Error("Refresh failed");

        const newAccessToken = await response.json();

        localStorage.setItem("accessToken", newAccessToken);

        console.log("Token refreshed successfully");
        return true;
    } catch (error) {
        console.error("Token refresh failed:", error);
        return false;
    }
}

function logout() {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    console.log("You have been logged out");
    window.location.href = "/login";
}

async function fetchWithAuth(url, options = {}, isBlob = false) {
    const accessToken = localStorage.getItem("accessToken");

    if (!options.headers) options.headers = {};
    if (accessToken) {
        options.headers["Authorization"] = `Bearer ${accessToken}`;
    }

    if (options.workspaceId) {
        options.headers["X-Workspace-Id"] = options.workspaceId;
        delete options.workspaceId;
    }

    if (options.boardId) {
        options.headers["X-Board-Id"] = options.boardId;
        delete options.boardId;
    }

    if (!options.isFormData) {
        options.headers["Content-Type"] = "application/json";
    }

    try {
        let response = await fetch(BASE_URL + url, options);

        if (response.status === 401) {
            console.log("Access token expired, attempting refresh");
            const refreshed = await refreshToken();

            if (refreshed) {
                const newAccessToken = localStorage.getItem("accessToken");
                options.headers["Authorization"] = `Bearer ${newAccessToken}`;

                console.log("Retrying request with new access token");
                response = await fetch(BASE_URL + url, options);

                if (!response.ok) {
                    throw new Error(`Request failed after token refresh with status ${response.status}`);
                }
            } else {
                console.log("Token refresh failed, logging out");
                logout();
                return null;
            }
        }

        if (!response.ok) {

            const err = new Error(response.message);

            console.log(response)
            err.errors = (await response.json())["errors"];
            err.status = response.status;
            throw err;

        }

        if (response.status === 204) {
            return { status: response.status };
        }

        if (isBlob) {
            return {
                blob: await response.blob(),
                contentDisposition: response.headers.get('Content-Disposition')
            };
        }

        return await response.json();
    } catch (error) {
        throw error;
    }
}

export const api = {
    get: (url, workspaceId = null) =>
        fetchWithAuth(url, { workspaceId }),

    getBlob: (url, workspaceId = null) =>
        fetchWithAuth(url, { workspaceId }, true),

    post: (url, data, workspaceId = null, boardId = null) =>
        fetchWithAuth(url, {
            method: 'POST',
            body: JSON.stringify(data),
            workspaceId,
            boardId
        }),

    postFormData: (url, data, workspaceId = null) =>
        fetchWithAuth(url, {
            method: 'POST',
            body: data,
            isFormData: true,
            workspaceId
        }),

    put: (url, data, workspaceId = null, boardId = null) =>
        fetchWithAuth(url, {
            method: 'PUT',
            body: JSON.stringify(data),
            workspaceId,
            boardId
        }),

    delete: (url, workspaceId = null, boardId = null) =>
        fetchWithAuth(url, {
            method: 'DELETE',
            workspaceId,
            boardId
        })
};