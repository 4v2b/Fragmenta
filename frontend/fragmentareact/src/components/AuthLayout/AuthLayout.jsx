import { Outlet, Navigate } from "react-router";
import { useAuth } from "@/utils/useAuth";
import { LanguageSwitch } from "../LanguageSwitch/LanguageSwitch";
import { Button } from "../Button/Button";
import { useTranslation } from "react-i18next";
import { logout } from "../../api/api";
import { Navbar } from "../Navbar/Navgar";
import "./AuthLayout.css"
import { useEffect, useState } from "react";
import { refreshToken } from "../../api/fetchClient";

export function AuthLayout() {
    // TODO Normal auth verification
    const { t } = useTranslation()
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

    return auth
        ? <div className="auth-layout">
            <Navbar>
                <Button content={t("fields.logout")} onClick={() => logout()} />
                <LanguageSwitch />
            </Navbar>
            <Outlet />
        </div>
        // : <>Loading</> 
        : <Navigate to="/login" replace />;
}