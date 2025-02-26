import { useState, useEffect } from "react";
import "./Login.css"
import { useTranslation } from 'react-i18next';
import { login } from "../../api/api"
import { Navigate, Link } from "react-router";
import { Button } from "../../components/Button/Button";
import { Form } from "../../components/Form/Form";
import { Input } from "../../components/Input/Input";


export function Login() {
    const [email, setEmail] = useState("")
    const [password, setPassword] = useState("")
    const [errors, setErrors] = useState({})
    const [skip, setSkip] = useState(localStorage.getItem("accessToken") ? true : false)

    async function handleLogin() {
        const response = await login(email, password)
        if (response.status == 200) {
            setSkip(true)
        }
        console.log(response)
    }

    const { t } = useTranslation();

    return !skip ? (
        <div className="login-content">
            <Form>
                <Input type="email" onInput={(e) => setEmail(e.target.value)} placeholder={t("fields.email")} />
                <Input type="password" onInput={(e) => setPassword(e.target.value)} placeholder={t("fields.password")} />
                <Link to="/register" >{t("auth.toRegister")}</Link>
                <Button onClick={handleLogin} content={t("fields.login")} />
            </Form>
        </div>) : (<Navigate to="/" replace />)

} 