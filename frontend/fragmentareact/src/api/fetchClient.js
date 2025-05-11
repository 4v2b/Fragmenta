// // api/fetchClient.js
// const BASE_URL = import.meta.env.VITE_API_URL;

// // Frontend fetch client
// async function fetchWithCookies(url, options = {}) {
//     let response = await fetch(BASE_URL + url, {
//         ...options,
//         credentials: 'include'
//     });

//     if (response.status === 401) {
//         // Try refresh
//         const refreshResponse = await fetch(BASE_URL + '/refresh', {
//             method: 'POST',
//             credentials: 'include'
//         });

//         if (refreshResponse.ok) {
//             // Retry original request - new JWT cookie will be included automatically
//             response = await fetch(BASE_URL + url, {
//                 ...options,
//                 credentials: 'include'
//             });
//         } else {
//             // Handle failed refresh (e.g., redirect to login)
//             window.location.href = '/login';
//         }
//     }

//     return response;
// };

// function logout() {
//     console.log("You have been logged out")
//     localStorage.removeItem("accessToken");
//     localStorage.removeItem("refreshToken");
//     window.location.href = "/login"; // Redirect to login page
// }

// async function fetchWithJwtBearer(url, options = {}, isBlob = false) {

//     const accessToken = localStorage.getItem("accessToken");

//     if (!options.headers) options.headers = {};

//     options.headers["Authorization"] = `Bearer ${accessToken}`;

//     console.log("fetching data from ", url, "with data ", options)

//     let response = await fetch(BASE_URL + url, options);

//     if (response.status === 401) {
//         try {
//             console.log("Try to refresh token")
//             await refreshToken();
//             options.headers["Authorization"] = `Bearer ${localStorage.getItem("accessToken")}`;
//             response = await fetch(BASE_URL + url, options);

//             if (!response.ok) {
//                 throw Error()
//             }
//             console.log(response, "from url ", url)
//             return await response.json();

//         } catch (error) {
//             console.log(error)
//             logout();
//             return;
//         }
//     }

//     if (response.status == 204) {
//         return { status: response.status }
//     }

//     console.log(response)

//     return isBlob ? {blob: await response.blob(), contentDisposition: response.headers.get('Content-Disposition') } : await response.json()
// };

// export async function refreshToken() {
//     const refreshToken = localStorage.getItem("refreshToken");

//     console.log("Current refresh token", refreshToken)
//     if (!refreshToken) return false;

//     const response = await fetch(BASE_URL + "/refresh", {
//         method: "POST",
//         headers: { "Content-Type": "application/json" },
//         body: JSON.stringify({ refreshToken })
//     });

//     console.log("Refresh response", response)

//     if (!response.ok) {
//         logout();
//     }

//     const data = await response.json();

//     console.log("Access token", data)
//     localStorage.setItem("accessToken", data);
// }

// export const api = {
//     get: (url, workspaceId = null) => fetchWithJwtBearer(url, {
//         headers: workspaceId ? {
//             "Content-Type": "application/json",
//             "X-Workspace-Id": workspaceId
//         } : {"Content-Type": "application/json"}
//     }),
//     getBlob: (url, workspaceId = null) => fetchWithJwtBearer(url, {
//         headers: workspaceId ? {
//             "X-Workspace-Id": workspaceId
//         } : {}
//     }, true),
//     post: (url, data, workspaceId = null) => fetchWithJwtBearer(url, {
//         headers: workspaceId ? {
//             "Content-Type": "application/json",
//             "X-Workspace-Id": workspaceId
//         } : {"Content-Type": "application/json"},
//         method: 'POST',
//         body: JSON.stringify(data)
//     }),
//     postFormData: (url, data, workspaceId = null) => fetchWithJwtBearer(url, {
//         headers: workspaceId ? {
//             "X-Workspace-Id": workspaceId
//         } : {},
//         method: 'POST',
//         body: data
//     }),
//     put: (url, data, workspaceId = null) => fetchWithJwtBearer(url, {
//         headers: workspaceId ? {
//             "Content-Type": "application/json",
//             "X-Workspace-Id": workspaceId
//         } : {"Content-Type": "application/json"},
//         method: 'PUT',
//         body: JSON.stringify(data)
//     }),
//     delete:
//         (url, workspaceId = null) => fetchWithJwtBearer(url, {
//             headers: workspaceId ? {
//                 "Content-Type": "application/json",
//                 "X-Workspace-Id": workspaceId
//             } : {"Content-Type": "application/json"},
//             method: 'DELETE'
//         })
//     // Add other methods as needed
// };


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

        // Your API returns just the access token as a string
        localStorage.setItem("accessToken", newAccessToken);
        // Keep using the same refresh token, no need to update it

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

    // Add workspace header if provided
    if (options.workspaceId) {
        options.headers["X-Workspace-Id"] = options.workspaceId;
        delete options.workspaceId;
    }

    if (options.boardId) {
        options.headers["X-Board-Id"] = options.boardId;
        delete options.boardId;
    }

    // Common content-type header unless it's FormData
    if (!options.isFormData) {
        options.headers["Content-Type"] = "application/json";
    }

    try {
        let response = await fetch(BASE_URL + url, options);

        // Handle token expiration
        if (response.status === 401) {
            console.log("Access token expired, attempting refresh");
            const refreshed = await refreshToken();

            if (refreshed) {
                // Update auth header with new token
                const newAccessToken = localStorage.getItem("accessToken");
                options.headers["Authorization"] = `Bearer ${newAccessToken}`;

                // Retry original request
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
        console.error("API request failed:", error);
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