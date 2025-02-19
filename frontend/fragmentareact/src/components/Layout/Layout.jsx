import { Outlet } from "react-router";
import { LanguageSwitch } from "../LanguageSwitch/LanguageSwitch";

export function Layout() {

    return <>
        <div className="corner-button">
            <LanguageSwitch />
        </div>
        <div className="main-content">
            <Outlet />
        </div>
    </>

}