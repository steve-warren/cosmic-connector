import http from 'k6/http';
import { sleep } from 'k6';

export default function () {
  http.get('http://localhost:5062/api/todo/a_2XcnUpI87sTHRKbeDBEqov2Qekd/lists');
  sleep(1);
}