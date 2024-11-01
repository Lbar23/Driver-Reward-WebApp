export interface PasswordRequirement {
    check: (pw: string) => boolean;
    message: string;
  }
  
  export const getPasswordRequirements = (userRole: string = 'Driver'): PasswordRequirement[] => {
    switch (userRole) {
      case 'Admin':
        return [
          { check: (pw: string) => pw.length >= 20, message: "At least 20 characters long" },
          { check: (pw: string) => /[0-9]/.test(pw), message: "Contains at least one digit" },
          { check: (pw: string) => /[a-z]/.test(pw), message: "Contains at least one lowercase letter" },
          { check: (pw: string) => /[A-Z]/.test(pw), message: "Contains at least one uppercase letter" },
          { check: (pw: string) => /[^A-Za-z0-9]/.test(pw), message: "Contains at least three symbols" },
          { check: (pw: string) => new Set(pw).size >= 3, message: "Contains at least 3 unique characters" },
          { check: (pw: string) => /^(?=.*[!@#$%^&*])(?=.*[0-9])(?=.*[A-Z]).*$/.test(pw), message: "Combination of numbers, symbols, and uppercase" },
        ];
      case 'Driver':
        return [
          { check: (pw: string) => pw.length >= 8, message: "At least 8 characters long" },
          { check: (pw: string) => /[0-9]/.test(pw), message: "Contains at least one digit" },
          { check: (pw: string) => /[a-z]/.test(pw), message: "Contains at least one lowercase letter" },
          { check: (pw: string) => /[A-Z]/.test(pw), message: "Contains at least one uppercase letter" },
          { check: (pw: string) => /[^A-Za-z0-9]/.test(pw), message: "Contains at least one special character" },
          { check: (pw: string) => new Set(pw).size >= 1, message: "Contains at least 1 unique character" },
        ];
      default: // Just in case, really...default cases goes brazy
        return [
          { check: (pw: string) => pw.length >= 6, message: "At least 6 characters long" },
          { check: (pw: string) => /[0-9]/.test(pw), message: "Contains at least one digit" },
          { check: (pw: string) => /[a-zA-Z]/.test(pw), message: "Contains at least one letter" },
          { check: (pw: string) => new Set(pw).size >= 1, message: "Contains at least 1 unique character" },
        ];
    }
  };