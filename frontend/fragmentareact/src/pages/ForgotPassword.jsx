import { useState, useEffect } from "react";
import { useTranslation } from 'react-i18next';
import { Navigate, Link } from "react-router";
import {
    Box,
    Button,
    Heading,
    VStack,
    Text,
    Input,
    Stack,
} from "@chakra-ui/react";
import { requestResetPassword } from "@/api/api";

export function ForgotPassword() {
    const [letterSent, setLetterSent] = useState(false);
    const [email, setEmail] = useState("");
    const [error, setError] = useState("");
    const [lockoutUntil, setLockoutUntil] = useState(null);
    const [timeLeft, setTimeLeft] = useState(0);
    const { t, i18n } = useTranslation();


    useEffect(() => {
        if (lockoutUntil) {
            const interval = setInterval(() => {
                const remaining = Math.max(0, Math.ceil((lockoutUntil - Date.now()) / 1000));
                setTimeLeft(remaining);

                if (remaining <= 0) {
                    setLockoutUntil(null);
                    clearInterval(interval);
                }
            }, 1000);

            return () => clearInterval(interval);
        }
    }, [lockoutUntil]);

    async function handleSendEmail() {
        const response = await requestResetPassword(email, i18n.language);
        if (response.status == 200) {
            setLetterSent(true);
            return;
        }

        setError(t(response.message));

        if (response.status === 423) {
            setLockoutUntil(response.lockoutUntil);
            setTimeLeft(Math.ceil((response.lockoutUntil - Date.now()) / 1000));
        }
    }

    return <Box bg="bg.subtle" maxW="md" mx="auto" mt={10} p={8} borderWidth={1} borderRadius="lg" boxShadow="lg">
        <VStack spacing={6} align="stretch">
            <Heading size="lg" textAlign="center" mb={2}>
                {t("auth.forgotPassword")}
            </Heading>

            <Stack spacing={4}>
                <Box>
                    <Text mb={2} fontWeight="medium">{t("fields.labels.email")}</Text>
                    <Input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        placeholder="your.email@example.com"
                        size="md"
                    />
                </Box>
            </Stack>

            {error && <Text color="red.500" fontSize="sm">{error}</Text>}

            {lockoutUntil && (
                <Text color="red.500" fontSize="sm">
                    {t("auth.errors.timeLeft", { time: timeLeft })}
                </Text>
            )}

            <Button
                onClick={handleSendEmail}
                colorScheme="blue"
                type="submit"
                width="full"
                size="lg"
                isDisabled={lockoutUntil}
                mt={2}
            >
                {t("auth.sendLetter")}
            </Button>

            <Text fontSize="sm" textAlign="center" mt={2}>
                <Link to={"/login"} color="blue">{t("auth.prompts.login")}</Link>
            </Text>
        </VStack>
    </Box>;
}