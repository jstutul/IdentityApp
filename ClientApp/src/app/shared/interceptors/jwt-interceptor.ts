import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Account } from '../../services/account';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(Account);

  const jwt = accountService.getJWT();

  if (jwt) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${jwt}`
      }
    });
  }

  console.log(req.headers.get('Authorization'));

  return next(req);
};