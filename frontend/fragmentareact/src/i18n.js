import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

// import Backend from 'i18next-http-backend';
// import LanguageDetector from 'i18next-browser-languagedetector';
// don't want to use this?
// have a look at the Quick start guide 
// for passing in lng and translations on init

export function changeLanguage(lng) {
    i18n.changeLanguage(lng);
    localStorage.setItem('userLanguage', lng);
};

i18n
    // .use(Backend)

    // .use(LanguageDetector)
    .use(initReactI18next)
    .init({
        fallbackLng: 'en',
        debug: true,
        lng: localStorage.getItem('userLanguage') || 'en',
        interpolation: {
            escapeValue: false, // not needed for react as it escapes by default
        },
        resources: {
            en: {
                translation: {

                    "greetings": {
                        "morning": "Good morning, {{username}}!",
                        "afternoon": "Good afternoon, {{username}}!",
                        "evening": "Good evening, {{username}}!",
                        "night": "Good night, {{username}}!"
                    },

                    roles: {
                        admin: "Admin",
                        owner: "Owner",
                        member: "Member",
                        guest: "Guest"
                    },
                    errors: {
                        workspaceExists: "Робочий простір з таким іменем вже існує",
                        fieldEmpty: "Fields cannot be empty",
                        userNotFound: "Users not found",
                        fileTooLarge: "File '{{filename}}' is too large",
                        forbiddenFileType: "Extension of file'{{filename}}' is forbidden in this board"
                    },
                    common: {
                        lastOpened: "Last opened",
                        accountDeleteHint: "Cannot delete account if you are a member of any workspace or board",
                        autoDeletion: "Will be removed in {{count}} day",
                        autoDeletion_plural: "Will be removed in {{count}} days",
                        autoDeletion_0: "Will be removed today",
                        workspaces: "Workspaces",
                        home: "Home",
                        archiveBoards: "Archived boards",
                        activeBoards: "Active boards",
                        settings: "Settings",
                        emptyBoard: "The board is empty",
                        noTasksYet: "No tasks yet",
                        greeting: "Welcome on the main page 🤗",
                        createWorkspace: "Create workspace",
                        workspaceStub: "None",
                        boards: "Boards",
                        members: "Members",
                        guests: "Guests list",
                        general: "General",
                        recent: "Recent Boards",
                        myWorkspaces: "Your Workspaces",
                    },
                    fields: {
                        priority: {
                            priority0: "None",
                            priority1: "High",
                            priority2: "Medium",
                            priority3: "Low",
                        },
                        labels: {
                            changeName: "Change your name",
                            incorrectPassword: "Incorrect password",
                             passwordHelp: "Must include upper and lowercase letters, and either numbers or symbols @#$%^&*!?",
                            changePassword: "Change password",
                            oldPassword: "Old password",
                            display: "View",
                            searchUser: "Find user by email",
                            Documents: "Documents",
                            Images: "Images",
                            Design: "Design",
                            Code: "Code",
                            Audio: "Audio",
                            Video: "Video",
                            Data: "Data",
                            Archives: "Archives",
                            taskLimit: "Tasks limit",
                            optional: "Optional",
                            color: "Color",
                            archivedEmptyTitle: "There is no archived boards yet",
                            createBoard: "Create a board",
                            assignedTo: "Assignee: {{name}}",
                            allowedAttachmentTypes: "Allowed file extensions",
                            attachments: "Attachments",
                            dragFile: "Drag and drop files here",
                            fileConstraint: "for files up to 10MB",
                            newPassword: "New password",
                            required: "This field is required",
                            statusExists: " Status with this name alredy exists",
                            addTask: "Add task",
                            addStatus: "Add status",
                            selPriority: "Select prioirty",
                            title: "Title",
                            settings: "Settings",
                            desc: "Description",
                            tags: "Tags",
                            upload: "Upload",
                            priority: "Prioirity",
                            dueDate: "Due date",
                            assignee: "Assignee",
                            editStatus: "Editings status",
                            name: "Name",
                            username: "Name",
                            role: "Role",
                            kick: "Kick",
                            email: "Email",
                            password: "Password",
                            repeatPassword: "Repeat password",
                            taskLimitInfo: "Limit amout of simultaneous tasks in the column",
                        },
                        actions: {
                            leaveWorkspace: "Leave workspace",
                            deleteWorkspace: "Delete workspace",
                            newStatus: "New status",
                            newBoard: "New board",
                            archive: "Archive",
                            grant: "Are you sure you want to grant admin permissions to user {{user}}?",
                            revoke: "Are you sure you want to revoke admin permissions from user {{user}}?",
                            confirm: "Confirm",
                            restore: "Restore",
                            areYouSure: "Are you sure?",
                            areYouSureAccount: "Are you sure you want to delete the account?",
                            cannotUndone: "This action cannot be undone",
                            cancel: "Cancel",

                            save: "Save",
                            deleteAccount: "Delete account",
                            create: "Create",
                            login: "Login",
                            register: "Register",
                            logout: "Log out",
                            delete: "Delete",
                            addMembers: "Add members",
                            findUser: "Find user"
                        }
                    },
                    auth: {
                        success: {
                            resetPassword: "Password was successfully changed",
                            loginRedirect: "Redirecting to login page",
                        },
                        resetPassword: "Creating a new password",
                        login: "Log in",
                        sendLetter: "Send a letter for password reset",
                        register: "Sign up",
                        forgotPassword: "Password reset",
                        stub: "Невірний логін або пароль",
                        errors: {
                            invalidResetToken: "The link is invalid or outdated",
                            passwordMismatch: "Passwords don't match",
                            userExists: "User with such email already exists",
                            passwordTooShort: "Password is too short",
                            passwordTooWeak: "Password is too weak",
                            passwordInvalid: "Invalid password",
                            emailInvalid: "Wrong format of email",
                            emailDoesntExist: "No user with this email",
                            lockout: "Too many unsuccessful inputs. Try again later",
                            timeLeft: "{{time}} left to next try",
                        },
                        prompts: {
                            backToLogin: "Don't want to reset password",
                            changePassword: "Change password",
                            register: "Sign Up",
                            login: "Login",
                            haveAccount: "Already have an account?",
                            noAccount: "Do not have any account?",
                            forgotPassword: "Forgot password?",
                        }
                    }
                }
            },
            uk: {
                translation: {


                    greetings: {
                        "morning": "Добрий ранок, {{username}}!",
                        "afternoon": "Добрий день, {{username}}!",
                        "evening": "Добрий вечір, {{username}}!",
                        "night": "Доброї ночі, {{username}}!"
                    },
                    roles: {
                        admin: "Адмін",
                        owner: "Власник",
                        member: "Учасник",
                        guest: "Гість"
                    },
                    errors: {
                        workspaceExists: "Робочий простір з таким іменем вже існує",
                        fieldEmpty: "Поле не може бути порожнім",
                        userNotFound: "Користувачів не знайдено",
                        fileTooLarge: "Файл '{{filename}}' завеликий",
                        forbiddenFileType: "Розширення файлу '{{filename}}' заборонене на цій дошці"
                    },
                    common: {
                        lastOpened: "Востаннє відкрито",
                        accountDeleteHint: "Неможливо видалити обліковий запис, якщо ви є учасником робочого простору або дошки",
                        autoDeletion_0: "Буде видалено сьогодні",
                        autoDeletion_one: "Буде видалено через {{count}} день",
                        autoDeletion_few: "Буде видалено через {{count}} дні",
                        autoDeletion_many: "Буде видалено через {{count}} днів",
                        autoDeletion_other: "Буде видалено через {{count}} дня",
                        home: "Головна",
                        noTasksYet: "Поки що немає завдань",
                        emptyBoard: "Ця дошка поки що порожня",
                        greeting: "Вітаємо на головній сторінці 🤗",
                        createWorkspace: "Новий робочий простір",
                        workspaceStub: "Не обрано",
                        workspaces: "Робочі простори",
                        boards: "Дошки",
                        activeBoards: "Активні дошки",
                        archiveBoards: "Архівовані дошки",
                        settings: "Налаштування",
                        guests: "Список гостей",
                        members: "Учасники",
                        general: "Загальне",
                        recent: "Нещодавні дошки",
                        myWorkspaces: "Ваші робочі простори",

                    },
                    fields: {
                        priority: {
                            priority0: "Немає",
                            priority1: "Високий",
                            priority2: "Середній",
                            priority3: "Низький",
                        },
                        labels: {
                            deleteStatus: "Видалити статус",
                            deleteTask: "Видалити завдання",
                            incorrectPassword: "Невірний пароль",
                            passwordHelp: "Має включати маленькі, великі літери, та числа або символи @#$%^&*!?",
                            changePassword: "Змінити пароль",
                            oldPassword: "Старий пароль",
                            changeName: "Змінити ім'я",
                            display: "Вигляд",
                            settings: "Налаштування",
                            searchUser: "Додати користувача за ел. поштою",
                            createBoard: "Створення дошки",
                            Documents: "Документи",
                            Images: "Зображення",
                            Design: "Дизайн",
                            Code: "Програмний код",
                            Audio: "Аудіо",
                            optional: "Необов'язково",
                            Video: "Відео",
                            Data: "Дані",
                            Archives: "Архіви",
                            addStatus: "Створення статусу",
                            editStatus: "Редагування статусу",
                            archivedEmptyTitle: "Немає архівованих дошок",
                            assignedTo: "Виконавець: {{name}}",
                            attachments: "Вкладені файли",
                            dragFile: "Перетяніть файл сюди",
                            fileConstraint: "для файлу розміром до 10МБ",
                            required: "Це поле є обов'язковим",
                            addTask: "Додати завдання",
                            selPriority: "Оберіть пріоритет",
                            title: "Назва",
                            desc: "Опис",
                            upload: "Завантажити",
                            allowedAttachmentTypes: "Дозволені розширення",
                            tags: "Позначки",
                            dueDate: "Дата виконання",
                            assignee: "Виконавець",
                            priority: "Пріоритет",
                            name: "Ім'я",
                            username: "Ім'я",
                            role: "Роль",
                            kick: "Вигнати",
                            email: "Ел. пошта",
                            password: "Пароль",
                            newPassword: "Новий пароль",
                            repeatPassword: "Повторіть пароль",
                            taskLimit: "Ліміт завдань",
                            taskLimitInfo: "Обмежити одночасну кількість завдань в колонці",
                            color: "Колір",
                            statusExists: "Статус з таким іменем вже існує",
                        },
                        actions: {
                            leaveWorkspace: "Покинути робочий простір",
                            deleteWorkspace: "Видалити робочий простір",
                            grant: "Ви впевнені, що хочете надати права адміністратора користувачу {{user}}?",
                            revoke: "Ви впевнені, що хочете відкликати права адміністратора в користувача {{user}}?",
                            confirm: "Підтвердити",
                            archive: "Архівувати",
                            restore: "Відновити",
                            areYouSure: "Ви впевнені?",
                            cannotUndone: "Цю дію не можна скасувати",
                            cancel: "Відмінити",
                            save: "Зберегти",
                            create: "Створити",
                            newBoard: "Нова дошка",
                            newStatus: "Новий статус",
                            login: "Увійти",
                            areYouSureAccount: "Впевнені, що хочете видалити обліковий запис?",
                            register: "Зареєструватися",
                            logout: "Вийти",
                            delete: "Видалити",
                            deleteAccount: "Видалити обліковий запис",
                            addMembers: "Додати учасників",
                            findUser: "Знайти користувача"
                        }
                    },
                    auth: {
                        success: {
                            resetPassword: "Пароль успішно змінено",
                            loginRedirect: "Вас буде перенаправлено на сторінку входу",
                        },
                        forgotPassword: "Скидання паролю",
                        resetPassword: "Створення нового паролю",
                        sendLetter: "Надіслати лист",
                        login: "Вхід",
                        register: "Реєстрація",
                        stub: "Невірний логін або пароль",
                        errors: {
                            invalidResetToken: "Посилання невірне або час на нього вичерпався",
                            passwordMismatch: "Паролі не співпадають",
                            userExists: "Користувач з такою ел. поштою вже існує",
                            passwordTooShort: "Пароль закороткий",
                            passwordTooWeak: "Пароль занадто слабкий",
                            passwordInvalid: "Невірний пароль",
                            emailInvalid: "Невірний формат ел. пошти",
                            userDoesntExist: "Користувача з даною ел. поштою не існує",
                            lockout: "Забагато невдалих введень. Спробуйте пізніше",
                            timeLeft: "Залишилось {{time}} до наступної спроби",
                        },
                        prompts: {
                            backToLogin: "Не хочу відновлювати пароль",
                            changePassword: "Змінити пароль",
                            login: "Увійти",
                            register: "Створіть новий",
                            haveAccount: "Вже маєте обліковий запис?",
                            noAccount: "Не маєте облікового запису?",
                            forgotPassword: "Забули пароль?",
                        }
                    }
                }
            }
        }
    });


export default i18n;