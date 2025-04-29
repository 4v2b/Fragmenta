import { useState, useEffect } from "react";
import { useTranslation } from 'react-i18next';
import { login } from "@/api/api";
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
import { Toaster, toaster } from "@/components/ui/toaster"

export function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [skip, setSkip] = useState(localStorage.getItem("accessToken") ? true : false);
  const [error, setError] = useState("");
  const [lockoutUntil, setLockoutUntil] = useState(null);
  const [timeLeft, setTimeLeft] = useState(0);
  const { t } = useTranslation();

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

  async function handleLogin() {

    if(email == "" || password == ""){
      setError(t("errors.fieldEmpty"))
      return;
    }


    const response = await login(email, password);
    if (response.status == 200) {
      setSkip(true);
      return;
    }

    setError(t(response.error));

    if (response.status === 423) {
      setLockoutUntil(response.lockoutUntil);
      setTimeLeft(Math.ceil((response.lockoutUntil - Date.now()) / 1000));
    }
  }

  return !skip ? (
    <Box bg="bg.subtle" maxW="md" mx="auto" mt={10} p={8} borderWidth={1} borderRadius="lg" boxShadow="lg">
      <VStack spacing={6} align="stretch">
        <Heading size="lg" textAlign="center" mb={2}>
          {t("auth.login")}
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

          <Box>
            <Text mb={2} fontWeight="medium">{t("fields.labels.password")}</Text>
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              size="md"
            />
          </Box>
          <Link to={"/forgot-password"} color="blue">{t("auth.prompts.forgotPassword")}</Link>
        </Stack>

        {error && <Text color="red.500" fontSize="sm">{error}</Text>}

        {lockoutUntil && (
          <Text color="red.500" fontSize="sm">
            {t("auth.errors.timeLeft", { time: timeLeft })}
          </Text>
        )}

        <Button
          onClick={handleLogin}
          bg="primary"
          type="submit"
          width="full"
          size="lg"
          isDisabled={lockoutUntil}
          mt={2}
        >
          {t("fields.actions.login")}
        </Button>

        <Text fontSize="sm" textAlign="center" mt={2}>
          {t("auth.prompts.noAccount")} <Link to={"/register"} color="blue.500">{t("auth.prompts.register")}</Link>
        </Text>
      </VStack>
      <Toaster/>
    </Box>
  ) : (
    <Navigate to="/" replace />
  );
}