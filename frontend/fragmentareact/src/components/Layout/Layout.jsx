import { Outlet, Navigate } from "react-router";
import { LanguageSwitch } from "../LanguageSwitch/LanguageSwitch";
import { useEffect, useState } from "react";
import { refreshToken } from "../../api/fetchClient";


export function Layout() {
    const [auth, setAuth] = useState(localStorage.getItem("accessToken") ? true : false)

    useEffect(() => {

        async function refresh() {
            await refreshToken()

            if (localStorage.getItem("accessToken")) {
                setAuth(true)
            }
            else {
                console.log("Cannot refresh token")
            }
        }

        if (!auth) {
            refresh()
        }

    }, [auth])

    return !auth
        ? <>
            <div className="corner-button">
                <LanguageSwitch />
            </div>
            <div className="main-content">
                <Outlet />
            </div>
        </>
        : <Navigate to="/" replace />;
}