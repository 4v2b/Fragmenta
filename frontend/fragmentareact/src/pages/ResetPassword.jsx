import { useState } from "react";
import { useTranslation } from "react-i18next";
import { resetPassword } from "@/api/api";
import { Link, useNavigate, useSearchParams } from "react-router";
import { Toaster, toaster } from "@/components/ui/toaster"
import {
    Box,
    Button,
    Heading,
    VStack,
    Text,
    Input,
    Stack,
} from "@chakra-ui/react";

export function ResetPassword() {
    const [password, setPassword] = useState("");
    const [passwordCopy, setPasswordCopy] = useState("");
    const [success, setSuccess] = useState(false);
    const [searchParams] = useSearchParams();
    const [error, setError] = useState("");
    const { t } = useTranslation();
    const navigate = useNavigate()

    async function handleSubmit() {
        if (password !== passwordCopy) {
            setError(t("auth.errors.passwordMismatch"));
            return;
        }

        try {
            const userId = searchParams.get("userId");
            const token = searchParams.get("token")
            const response = await resetPassword(token, userId, password);
            if (response.status == 200) {

                toaster.create({
                    title: t("auth.success.resetPassword"),
                    description: t("auth.success.loginRedirect"),
                    type: "success",
                })

                setSuccess(true);
                setError("");
                setTimeout(() => {
                    navigate("/login");
                }, 3000);
                return;

            }

            setError(t(response.message))
        } catch (err) {
            setError(t("auth.errors.registrationFailed"));
        }
    }

    return <Box bg="bg.subtle" maxW="md" mx="auto" mt={10} p={8} borderWidth={1} borderRadius="lg" boxShadow="lg">
        <VStack spacing={6} align="stretch">
            <Heading size="lg" textAlign="center" mb={2}>
                {t("auth.resetPassword")}
            </Heading>

            <Stack spacing={4}>
                <Box>
                    <Text mb={2} fontWeight="medium">{t("fields.labels.newPassword")}</Text>
                    <Input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        placeholder={t("fields.labels.newPassword")}
                        size="md"
                    />
                </Box>

                <Box>
                    <Text mb={2} fontWeight="medium">{t("fields.labels.repeatPassword")}</Text>
                    <Input
                        type="password"
                        value={passwordCopy}
                        onChange={(e) => setPasswordCopy(e.target.value)}
                        placeholder={t("fields.labels.repeatPassword")}
                        size="md"
                    />
                </Box>
            </Stack>

            {error && <Text color="red.500" fontSize="sm">{error}</Text>}

            <Button
                onClick={handleSubmit}
                bg="primary"
                type="submit"
                width="full"
                size="lg"
                mt={2}
            >
                {t("auth.prompts.changePassword")}
            </Button>
            <Text fontSize="sm" textAlign="center" mt={2} color="blue.500">
                <Link to="/login" >{t("auth.prompts.backToLogin")}</Link>
            </Text>

        </VStack>
        <Toaster/>
    </Box>
}